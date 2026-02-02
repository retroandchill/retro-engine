/**
 * @file delegate.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include "retro/core/exports.h"

export module retro.core.functional.delegate;

import std;
import retro.core.functional.binding;
import retro.core.type_traits.variant;
import retro.core.containers.optional;
import retro.core.type_traits.callable;
import retro.core.type_traits.pointer;

namespace retro
{
    template <typename T>
    concept DirectMemberBindable = std::is_object_v<std::remove_cvref_t<T>> && std::is_lvalue_reference_v<T> &&
                                   !(SharedPtrLike<T> || WeakPtrLike<T>);

    export template <typename T>
    concept MemberBindable =
        DirectMemberBindable<T> ||
        ((SharedPtrLike<T> || WeakPtrLike<T>)&&std::is_object_v<PointerElementT<std::remove_cvref_t<T>>>);

    template <MemberBindable>
    struct MemberBinding;

    template <DirectMemberBindable T>
    struct MemberBinding<T>
    {
        using Type = T;
    };

    template <typename T>
        requires SharedPtrLike<T> || WeakPtrLike<T>
    struct MemberBinding<T>
    {
        using Type = PointerElementT<std::remove_cvref_t<T>> &;
    };

    template <MemberBindable T>
    using MemberBindingT = MemberBinding<T>::Type;

    template <WeakPtrLike T, typename Functor>
        requires std::is_member_pointer_v<Functor>
    struct WeakFunctionBinding
    {
        constexpr WeakFunctionBinding(T object, Functor functor)
            : object_(std::move(object)), functor_(std::move(functor))
        {
        }

        template <typename... Args>
            requires std::invocable<Functor, PointerElementT<T> &, Args...>
        constexpr decltype(auto) operator()(Args &&...args) const
            noexcept(std::is_nothrow_invocable_v<Functor, T &, Args...>)
        {
            return std::invoke(functor_, *object_.lock(), std::forward<Args>(args)...);
        }

        constexpr bool is_bound() const noexcept
        {
            return !object_.expired();
        }

      private:
        T object_;
        Functor functor_;
    };

    template <WeakPtrLike T, auto Functor>
        requires std::is_member_pointer_v<decltype(Functor)>
    struct ConstWeakFunctionBinding
    {
        constexpr explicit ConstWeakFunctionBinding(T object) : object_(std::move(object))
        {
        }

        template <typename... Args>
            requires std::invocable<decltype(Functor), PointerElementT<T> &, Args...>
        constexpr decltype(auto) operator()(Args &&...args) const
            noexcept(std::is_nothrow_invocable_v<decltype(Functor), T &, Args...>)
        {
            return std::invoke(Functor, *object_.lock(), std::forward<Args>(args)...);
        }

        constexpr bool is_bound() const noexcept
        {
            return !object_.expired();
        }

      private:
        T object_;
    };

    template <typename>
    struct IsWeakFunctionBinding : std::false_type
    {
    };

    template <WeakPtrLike T, typename Functor>
        requires std::is_member_pointer_v<Functor>
    struct IsWeakFunctionBinding<WeakFunctionBinding<T, Functor>> : std::true_type
    {
    };

    template <typename T>
    concept WeakFunctionBindingLike = IsWeakFunctionBinding<std::remove_cvref_t<T>>::value;

    static constexpr std::size_t DELEGATE_INLINE_ALIGN = alignof(std::max_align_t);
    static constexpr std::size_t DELEGATE_INLINE_SIZE = 16;

    export struct NoLockPolicy
    {
        struct Mutex
        {
        };

        struct ReadGuard
        {
        };

        struct WriteGuard
        {
        };
    };

    export struct SharedLockPolicy
    {
        using Mutex = std::shared_mutex;
        using ReadGuard = std::shared_lock<Mutex>;
        using WriteGuard = std::unique_lock<Mutex>;
    };

    template <typename Policy>
    class ThreadPolicyMixin
    {
      public:
        using Mutex = Policy::Mutex;
        using ReadGuard = Policy::ReadGuard;
        using WriteGuard = Policy::WriteGuard;

        [[nodiscard]] ReadGuard read_lock() const noexcept
        {
            return ReadGuard{mutex_};
        }

        [[nodiscard]] WriteGuard write_lock() noexcept
        {
            return WriteGuard{mutex_};
        }

        [[nodiscard]] Mutex &mutex() noexcept
        {
            return mutex_;
        }

      private:
        mutable Mutex mutex_{};
    };

    template <>
    class ThreadPolicyMixin<NoLockPolicy>
    {
      public:
        using Mutex = NoLockPolicy::Mutex;
        using ReadGuard = NoLockPolicy::ReadGuard;
        using WriteGuard = NoLockPolicy::WriteGuard;

        [[nodiscard]] constexpr ReadGuard read_lock() const noexcept
        {
            return ReadGuard{};
        }

        [[nodiscard]] constexpr WriteGuard write_lock() noexcept
        {
            return WriteGuard{};
        }
    };

    union DelegateStorage
    {
        alignas(DELEGATE_INLINE_ALIGN) std::byte inline_buffer[DELEGATE_INLINE_SIZE];
        void *heap_object{};
    };

    export template <typename, typename ThreadPolicy = NoLockPolicy>
    class Delegate;

    export template <typename Ret, typename... Args, typename Policy>
    class Delegate<Ret(Args...), Policy> : private ThreadPolicyMixin<Policy>
    {
      public:
        using ReturnType = Ret;
        using Signature = Ret(Args...);

        constexpr Delegate() = default;
        constexpr explicit(false) Delegate(std::nullptr_t)
        {
        }

        template <typename Functor>
            requires std::invocable<const std::remove_reference_t<Functor>, Args...> &&
                     std::convertible_to<std::invoke_result_t<const std::remove_reference_t<Functor>, Args...>, Ret>
        constexpr explicit(false) Delegate(Functor &&functor) : ops_(get_ops_table<std::remove_cvref_t<Functor>>())
        {
            store_functor(std::forward<Functor>(functor));
        }

        Delegate(const Delegate &other)
            requires std::same_as<NoLockPolicy, Policy>
            : ops_(other.ops_)
        {
            copy_data(other);
        }

        Delegate(const Delegate &other)
            requires(!std::same_as<NoLockPolicy, Policy>)
        {
            auto lock = other.read_lock();
            ops_ = other.ops_;
            copy_data(other);
        }

        constexpr Delegate(Delegate &&other) noexcept
            requires std::same_as<NoLockPolicy, Policy>
            : ops_(other.ops_)
        {
            move_data(std::move(other));
        }

        constexpr Delegate(Delegate &&other) noexcept
            requires(!std::same_as<NoLockPolicy, Policy>)
        {
            auto lock = other.write_lock();
            ops_ = other.ops_;
            move_data(std::move(other));
        }

        ~Delegate()
        {
            delete_data();
        }

        Delegate &operator=(const Delegate &other)
        {
            if (this == std::addressof(other))
                return *this;

            if constexpr (!std::same_as<Policy, NoLockPolicy>)
            {
                typename Policy::WriteGuard write_guard{this->mutex(), std::defer_lock};
                typename Policy::ReadGuard read_guard{other.mutex(), std::defer_lock};
                std::lock(read_guard, write_guard);

                delete_data();

                ops_ = other.ops_;
                copy_data(other);
                return *this;
            }
            else
            {
                delete_data();

                ops_ = other.ops_;
                copy_data(other);
                return *this;
            }
        }

        constexpr Delegate &operator=(Delegate &&other) noexcept
        {
            if constexpr (!std::same_as<Policy, NoLockPolicy>)
            {
                typename Policy::WriteGuard this_lock{this->mutex(), std::defer_lock};
                typename Policy::WriteGuard other_lock{other.mutex(), std::defer_lock};
                std::lock(this_lock, this_lock);

                delete_data();

                ops_ = other.ops_;
                move_data(std::move(other));
                return *this;
            }
            else
            {
                delete_data();

                ops_ = other.ops_;
                move_data(std::move(other));
                return *this;
            }
        }

        constexpr Delegate &operator=(std::nullptr_t) noexcept
        {
            unbind();
            return *this;
        }

        template <typename Functor>
            requires std::invocable<const std::remove_reference_t<Functor>, Args...> &&
                     std::convertible_to<std::invoke_result_t<const std::remove_reference_t<Functor>, Args...>, Ret>
        constexpr Delegate &operator=(Functor &&functor)
        {
            bind(std::forward<Functor>(functor));
            return *this;
        }

        [[nodiscard]] constexpr bool is_bound() const noexcept
        {
            auto lock = this->read_lock();
            return is_bound_no_locks();
        }

        constexpr void unbind()
        {
            auto lock = this->write_lock();
            delete_data();
            ops_ = nullptr;
        }

        constexpr Ret execute(Args &&...args) const
        {
            auto lock = this->read_lock();
            if (!is_bound_no_locks())
            {
                throw std::bad_function_call{};
            }

            return ops_->invoke(storage_, std::forward<Args>(args)...);
        }

        constexpr bool execute_if_bound(Args... args) const
            requires std::same_as<Ret, void>
        {
            auto lock = this->read_lock();
            if (!is_bound_no_locks())
            {
                return false;
            }

            ops_->invoke(storage_, std::forward<Args>(args)...);
            return true;
        }
        constexpr Optional<Ret> execute_if_bound(Args... args) const
            requires !std::same_as<Ret, void>
        {
            auto lock = this->read_lock();
            if (!is_bound_no_locks())
            {
                return std::nullopt;
            }

            return ops_->invoke(storage_, std::forward<Args>(args)...);
        }

        template <typename Functor, typename... BindArgs>
            requires std::invocable<const std::remove_reference_t<Functor>, Args..., BindArgs...> &&
                     std::convertible_to<
                         std::invoke_result_t<const std::remove_reference_t<Functor>, Args..., BindArgs...>,
                         Ret>
        void bind(Functor &&functor, BindArgs &&...args) noexcept
        {
            if constexpr (sizeof...(BindArgs) > 0)
            {
                bind(std::bind_back(std::forward<Functor>(functor), std::forward<BindArgs>(args)...));
            }
            else
            {
                auto lock = this->write_lock();
                delete_data();

                ops_ = get_ops_table<std::remove_cvref_t<Functor>>();
                store_functor(std::forward<Functor>(functor));
            }
        }

        template <auto Functor, typename... BindArgs>
            requires std::invocable<decltype(Functor), Args..., BindArgs...> &&
                     std::convertible_to<std::invoke_result_t<decltype(Functor), Args..., BindArgs...>, Ret>
        void bind(BindArgs &&...args) noexcept
        {
            if constexpr (sizeof...(BindArgs) > 0)
            {
                return bind(bind_back<Functor>(std::forward<BindArgs>(args)...));
            }
            else
            {
                bind([]<typename... T>(T &&...a) { return std::invoke(Functor, std::forward<T>(a)...); });
            }
        }

        template <MemberBindable T, typename Member, typename... BindArgs>
            requires std::invocable<Member, MemberBindingT<T>, Args..., BindArgs...> &&
                     std::convertible_to<std::invoke_result_t<Member, MemberBindingT<T>, Args..., BindArgs...>, Ret> &&
                     std::is_member_pointer_v<Member>
        void bind(T &&obj, Member member, BindArgs &&...args) noexcept
        {
            if constexpr (WeakBindable<T>)
            {
                using WeakType = std::remove_cvref_t<decltype(to_weak(std::forward<T>(obj)))>;
                if constexpr (sizeof...(BindArgs) > 0)
                {
                    bind(std::bind_back(WeakFunctionBinding<WeakType, Member>{to_weak(std::forward<T>(obj)), member},
                                        std::forward<BindArgs>(args)...));
                }
                else
                {
                    bind(WeakFunctionBinding<WeakType, Member>{to_weak(std::forward<T>(obj)), member});
                }
            }
            else
            {
                if constexpr (sizeof...(BindArgs) > 0)
                {
                    bind(std::bind_back(std::bind_front(member, std::ref(obj)), std::forward<BindArgs>(args)...));
                }
                else
                {
                    bind(std::bind_front(member, std::ref(obj)));
                }
            }
        }

        template <auto Member, MemberBindable T, typename... BindArgs>
            requires std::invocable<decltype(Member), MemberBindingT<T>, Args..., BindArgs...> &&
                     std::convertible_to<
                         std::invoke_result_t<decltype(Member), MemberBindingT<T>, Args..., BindArgs...>,
                         Ret> &&
                     std::is_member_pointer_v<decltype(Member)>
        void bind(T &&obj, BindArgs &&...args) noexcept
        {
            if constexpr (WeakBindable<T>)
            {
                using WeakType = std::remove_cvref_t<decltype(to_weak(std::forward<T>(obj)))>;
                if constexpr (sizeof...(BindArgs) > 0)
                {
                    bind(std::bind_back(ConstWeakFunctionBinding<WeakType, Member>{to_weak(std::forward<T>(obj))},
                                        std::forward<BindArgs>(args)...));
                }
                else
                {
                    bind(ConstWeakFunctionBinding<WeakType, Member>{to_weak(std::forward<T>(obj))});
                }
            }
            else
            {
                if constexpr (sizeof...(BindArgs) > 0)
                {
                    bind(std::bind_back(bind_front<Member>(std::ref(obj)), std::forward<BindArgs>(args)...));
                }
                else
                {
                    bind(bind_front<Member>(std::ref(obj)));
                }
            }
        }

        template <typename Functor, typename... BindArgs>
            requires std::invocable<const std::remove_reference_t<Functor>, Args..., BindArgs...> &&
                     std::convertible_to<
                         std::invoke_result_t<const std::remove_reference_t<Functor>, Args..., BindArgs...>,
                         Ret>
        static Delegate create(Functor &&functor, BindArgs &&...args) noexcept
        {
            Delegate delegate;
            delegate.bind(std::forward<Functor>(functor), std::forward<BindArgs>(args)...);
            return delegate;
        }

        template <auto Functor, typename... BindArgs>
            requires std::invocable<decltype(Functor), Args..., BindArgs...> &&
                     std::convertible_to<std::invoke_result_t<decltype(Functor), Args..., BindArgs...>, Ret>
        static Delegate create(BindArgs &&...args) noexcept
        {
            Delegate delegate;
            delegate.template bind<Functor>(std::forward<BindArgs>(args)...);
            return delegate;
        }

        template <MemberBindable T, typename Member, typename... BindArgs>
            requires std::invocable<Member, MemberBindingT<T>, Args..., BindArgs...> &&
                     std::convertible_to<std::invoke_result_t<Member, MemberBindingT<T>, Args..., BindArgs...>, Ret> &&
                     std::is_member_pointer_v<Member>
        static Delegate create(T &&obj, Member member, BindArgs &&...args) noexcept
        {
            Delegate delegate;
            delegate.template bind<T, Member>(std::forward<T>(obj), member, std::forward<BindArgs>(args)...);
            return delegate;
        }

        template <auto Member, MemberBindable T, typename... BindArgs>
            requires std::invocable<decltype(Member), MemberBindingT<T>, Args..., BindArgs...> &&
                     std::convertible_to<
                         std::invoke_result_t<decltype(Member), MemberBindingT<T>, Args..., BindArgs...>,
                         Ret> &&
                     std::is_member_pointer_v<decltype(Member)>
        static Delegate create(T &&obj, BindArgs &&...args) noexcept
        {
            Delegate delegate;
            delegate.template bind<Member, T, BindArgs...>(std::forward<T>(obj), std::forward<BindArgs>(args)...);
            return delegate;
        }

      private:
        struct OpsTable
        {
            std::size_t object_size{};
            Ret (*invoke)(const DelegateStorage &storage, Args... args) = nullptr;
            void (*copy)(DelegateStorage &dest, const DelegateStorage &src) = nullptr;
            void (*destroy)(DelegateStorage &) = nullptr;
            bool (*is_bound)(const DelegateStorage &) = nullptr;
        };

        [[nodiscard]] constexpr bool is_bound_no_locks() const noexcept
        {
            return ops_ != nullptr && (ops_->is_bound == nullptr || ops_->is_bound(storage_));
        }

        [[nodiscard]] constexpr bool is_inline() const noexcept
        {
            return ops_ != nullptr && ops_->object_size <= DELEGATE_INLINE_SIZE;
        }

        void copy_data(const Delegate &other)
        {
            if (ops_ == nullptr)
                return;

            if (ops_->copy != nullptr)
            {
                ops_->copy(storage_, other.storage_);
            }
            else if (is_inline())
            {
                std::memcpy(storage_.inline_buffer, other.storage_.inline_buffer, sizeof(storage_.inline_buffer));
            }
        }

        void move_data(Delegate &&other)
        {
            if (ops_ == nullptr)
                return;

            if (is_inline())
            {
                std::memcpy(storage_.inline_buffer, other.storage_.inline_buffer, sizeof(storage_.inline_buffer));
            }
            else
            {
                storage_.heap_object = other.storage_.heap_object;
                other.storage_.heap_object = nullptr;
            }

            other.ops_ = nullptr;
        }

        void delete_data()
        {
            if (ops_ != nullptr && ops_->destroy != nullptr)
            {
                ops_->destroy(storage_);
            }
        }

        template <typename Functor>
        void store_functor(Functor &&functor)
        {
            if constexpr (sizeof(Functor) <= DELEGATE_INLINE_SIZE)
            {
                auto *functor_ptr =
                    std::launder(reinterpret_cast<std::remove_cvref_t<Functor> *>(&storage_.inline_buffer));
                std::construct_at(functor_ptr, std::forward<Functor>(functor));
            }
            else
            {
                storage_.heap_object = new Functor(std::forward<Functor>(functor));
            }
        }

        template <std::invocable<Args...> Functor>
            requires std::convertible_to<std::invoke_result_t<Functor, Args...>, Ret>
        static OpsTable *get_ops_table()
        {
            static OpsTable ops_table{.object_size = sizeof(Functor),
                                      .invoke = invoke_functor<Functor>,
                                      .copy = get_copy_operation<Functor>(),
                                      .destroy = get_delete_operation<Functor>(),
                                      .is_bound = get_bound_check<Functor>()};
            return &ops_table;
        }

        template <std::invocable<Args...> Functor>
            requires std::convertible_to<std::invoke_result_t<Functor, Args...>, Ret>
        static auto get_copy_operation()
        {
            if constexpr (!std::is_trivially_copyable_v<Functor> || sizeof(Functor) > DELEGATE_INLINE_SIZE)
            {
                return &copy_functor<Functor>;
            }
            else
            {
                return nullptr;
            }
        }

        template <std::invocable<Args...> Functor>
            requires std::convertible_to<std::invoke_result_t<Functor, Args...>, Ret>
        static auto get_delete_operation()
        {
            if constexpr (!std::is_trivially_destructible_v<Functor> || sizeof(Functor) > DELEGATE_INLINE_SIZE)
            {
                return &delete_functor<Functor>;
            }
            else
            {
                return nullptr;
            }
        }

        template <std::invocable<Args...> Functor>
            requires std::convertible_to<std::invoke_result_t<Functor, Args...>, Ret>
        static auto get_bound_check()
        {
            if constexpr (WeakFunctionBindingLike<Functor>)
            {
                return &is_functor_bound<Functor>;
            }
            else
            {
                return nullptr;
            }
        }

        template <typename Functor>
        static Ret invoke_functor(const DelegateStorage &obj, Args... args)
        {
            if constexpr (sizeof(Functor) <= DELEGATE_INLINE_SIZE)
            {
                return std::invoke(*std::launder(reinterpret_cast<const Functor *>(&obj.inline_buffer)),
                                   std::forward<Args>(args)...);
            }
            else
            {
                return std::invoke(*static_cast<const Functor *>(obj.heap_object), std::forward<Args>(args)...);
            }
        }

        template <typename Functor>
        static void copy_functor(DelegateStorage &dest, const DelegateStorage &source)
        {
            if constexpr (sizeof(Functor) <= DELEGATE_INLINE_SIZE)
            {
                std::construct_at(reinterpret_cast<Functor *>(dest.inline_buffer),
                                  *reinterpret_cast<const Functor *>(source.inline_buffer));
            }
            else
            {
                dest.heap_object = new Functor(*static_cast<const Functor *>(source.heap_object));
            }
        }

        template <typename Functor>
        static void delete_functor(DelegateStorage &obj)
        {
            if constexpr (sizeof(Functor) <= DELEGATE_INLINE_SIZE)
            {
                std::destroy_at(reinterpret_cast<Functor *>(obj.inline_buffer));
            }
            else
            {
                delete static_cast<Functor *>(obj.heap_object);
            }
        }

        template <WeakFunctionBindingLike Functor>
        static bool is_functor_bound(const DelegateStorage &obj)
        {
            if constexpr (sizeof(Functor) <= DELEGATE_INLINE_SIZE)
            {
                return std::launder(reinterpret_cast<Functor *>(&obj.inline_buffer))->is_bound();
            }
            else
            {
                return static_cast<const Functor *>(obj.heap_object)->is_bound();
            }
        }

        DelegateStorage storage_;
        const OpsTable *ops_ = nullptr;
    };

    export using SimpleDelegate = Delegate<void()>;

    export struct DelegateHandle
    {
        constexpr DelegateHandle() noexcept = default;
        constexpr DelegateHandle(const std::uint64_t owner_cookie,
                                 const std::uint32_t index,
                                 const std::uint32_t generation) noexcept
            : owner_cookie_(owner_cookie), index_(index), generation_(generation)
        {
        }

        [[nodiscard]] constexpr std::uint64_t owner_cookie() const noexcept
        {
            return owner_cookie_;
        }

        [[nodiscard]] constexpr std::uint32_t index() const noexcept
        {
            return index_;
        }

        [[nodiscard]] constexpr std::uint32_t generation() const noexcept
        {
            return generation_;
        }

        [[nodiscard]] constexpr bool is_valid() const noexcept
        {
            return owner_cookie_ != 0;
        }

        [[nodiscard]] static constexpr DelegateHandle invalid() noexcept
        {
            return DelegateHandle{};
        }

        [[nodiscard]] static inline std::uint64_t generate_new_cookie() noexcept
        {
            return next_cookie_.fetch_add(1, std::memory_order_relaxed);
        }

      private:
        std::uint64_t owner_cookie_{};
        std::uint32_t index_{};
        std::uint32_t generation_{};

        RETRO_API static std::atomic<std::uint64_t> next_cookie_;
    };

    export template <typename, typename Policy = NoLockPolicy>
    class MulticastDelegate;

    export template <typename... Args, typename Policy>
    class MulticastDelegate<void(Args...), Policy> : private ThreadPolicyMixin<Policy>
    {
      public:
        using DelegateType = Delegate<void(Args...), Policy>;

        MulticastDelegate() = default;

        MulticastDelegate(const MulticastDelegate &)
            requires std::same_as<Policy, NoLockPolicy>
        = default;

        MulticastDelegate(const MulticastDelegate &other)
            requires(!std::same_as<Policy, NoLockPolicy>)
            : cookie_{other.cookie_}
        {
            auto lock = other.read_lock();

            slots_ = other.slots_;
            free_list_ = other.free_list_;
        }

        MulticastDelegate(MulticastDelegate &&) noexcept
            requires std::same_as<Policy, NoLockPolicy>
        = default;

        MulticastDelegate(MulticastDelegate &&other) noexcept
            requires(!std::same_as<Policy, NoLockPolicy>)
        {
            auto lock = other.write_lock();

            slots_ = std::move(other.slots_);
            free_list_ = std::move(other.free_list_);
        }

        ~MulticastDelegate() = default;

        MulticastDelegate &operator=(const MulticastDelegate &)
            requires std::same_as<Policy, NoLockPolicy>
        = default;

        MulticastDelegate &operator=(const MulticastDelegate &other)
            requires(!std::same_as<Policy, NoLockPolicy>)
        {
            typename Policy::WriteGuard write_guard{this->mutex(), std::defer_lock};
            typename Policy::ReadGuard read_guard{other.mutex(), std::defer_lock};
            std::lock(write_guard, read_guard);

            slots_ = other.slots_;
            free_list_ = other.free_list_;
            return *this;
        }

        MulticastDelegate &operator=(MulticastDelegate &&) noexcept
            requires std::same_as<Policy, NoLockPolicy>
        = default;

        MulticastDelegate &operator=(MulticastDelegate &&other) noexcept
            requires(!std::same_as<Policy, NoLockPolicy>)
        {
            typename Policy::WriteGuard write_guard{this->mutex(), std::defer_lock};
            typename Policy::ReadGuard read_guard{other.mutex(), std::defer_lock};
            std::lock(write_guard, read_guard);

            cookie_ = other.cookie_;
            slots_ = std::move(other.slots_);
            free_list_ = std::move(other.free_list_);
            return *this;
        }

        DelegateHandle add(DelegateType delegate)
        {
            auto lock = this->write_lock();
            auto [index, slot] = allocate_slot(std::move(delegate));
            return DelegateHandle{cookie_, static_cast<std::uint32_t>(index), slot.generation};
        }

        template <typename Functor, typename... BindArgs>
            requires std::invocable<const std::remove_reference_t<Functor>, Args..., BindArgs...>
        DelegateHandle add(Functor &&functor, BindArgs &&...args) noexcept
        {
            return add(DelegateType::create(std::forward<Functor>(functor), std::forward<BindArgs>(args)...));
        }

        template <auto Functor, typename... BindArgs>
            requires std::invocable<decltype(Functor), Args..., BindArgs...>
        DelegateHandle add(BindArgs &&...args) noexcept
        {
            return add(DelegateType::template create<Functor>(std::forward<BindArgs>(args)...));
        }

        template <MemberBindable T, typename Member, typename... BindArgs>
            requires std::invocable<Member, MemberBindingT<T>, Args..., BindArgs...> && std::is_member_pointer_v<Member>
        DelegateHandle add(T &&obj, Member member, BindArgs &&...args) noexcept
        {
            return add(DelegateType::template create<T, Member>(std::forward<T>(obj),
                                                                member,
                                                                std::forward<BindArgs>(args)...));
        }

        template <auto Member, MemberBindable T, typename... BindArgs>
            requires std::invocable<decltype(Member), MemberBindingT<T>, Args..., BindArgs...> &&
                     std::is_member_pointer_v<decltype(Member)>
        DelegateHandle add(T &&obj, BindArgs &&...args) noexcept
        {
            return add(DelegateType::template create<Member, T>(std::forward<T>(obj), std::forward<BindArgs>(args)...));
        }

        void remove(const DelegateHandle handle)
        {
            if (!handle.is_valid() || handle.owner_cookie() != cookie_)
                return;

            auto lock = this->write_lock();
            free_slot(handle.index());
        }

        void clear()
        {
            auto lock = this->write_lock();
            slots_.clear();
            free_list_.clear();
        }

        void broadcast(Args... args) const
        {
            auto lock = this->read_lock();
            for (const auto [i, slot] : slots_ | std::views::enumerate)
            {
                if (!slot.is_bound)
                {
                    continue;
                }

                if (!slot.delegate.is_bound())
                {
                    free_slot(i);
                    continue;
                }

                slot.delegate.execute(std::forward<Args>(args)...);
            }
        }

        [[nodiscard]] std::size_t size() const noexcept
        {
            auto lock = this->read_lock();
            std::size_t count = 0;
            for (const auto &slot : slots_)
            {
                if (slot.is_bound)
                {
                    ++count;
                }
            }
            return count;
        }

      private:
        struct Slot
        {
            DelegateType delegate{};
            std::uint32_t generation{};
            bool is_bound{false};
        };

        std::pair<std::size_t, Slot &> allocate_slot(DelegateType delegate)
        {
            if (!free_list_.empty())
            {
                const std::size_t idx = free_list_.back();
                free_list_.pop_back();
                auto &slot = slots_[idx];
                slot.delegate = std::move(delegate);
                slot.generation = 0;
                return {idx, slot};
            }

            auto &slot = slots_.emplace_back(std::move(delegate), 0, true);
            return {slots_.size() - 1, slot};
        }

        void free_slot(std::size_t idx) const
        {
            auto &slot = slots_[idx];

            slot.delegate.unbind();
            slot.is_bound = false;
            ++slot.generation;

            free_list_.push_back(idx);
        }

        std::uint64_t cookie_{DelegateHandle::generate_new_cookie()};
        mutable std::vector<Slot> slots_{};
        mutable std::vector<std::size_t> free_list_{};
    };

    export using SimpleMulticastDelegate = MulticastDelegate<void()>;
} // namespace retro
