//
// Created by fcors on 12/25/2025.
//

export module retro.core.memory:ref_counted_ptr;

import retro.core.strings;
import retro.core;
import std;

namespace retro {
    export class RefCounted {
    public:
        virtual ~RefCounted() = default;

        virtual void retain() = 0;
        virtual void release() = 0;
    };


    export class IntrusiveRefCounted : public RefCounted {
    public:
        IntrusiveRefCounted() noexcept = default;

    protected:
        ~IntrusiveRefCounted() override = default;

    public:
        inline void retain() noexcept override {
            ref_count_.fetch_add(1, std::memory_order_relaxed);
        }

        inline void release() noexcept override {
            if (ref_count_.fetch_sub(1, std::memory_order_acq_rel) == 1) {
                delete this; // NOSONAR
            }
        }

        [[nodiscard]] inline uint32 ref_count() const noexcept {
            return ref_count_.load(std::memory_order_relaxed);
        }

    private:
        std::atomic<uint32> ref_count_{1};
    };

    export template <std::derived_from<RefCounted> T>
    class RefCountPtr {
    public:
        using element_type = T;

        constexpr RefCountPtr() noexcept = default;

        constexpr explicit(false) RefCountPtr(std::nullptr_t) noexcept {}

        explicit RefCountPtr(T* ptr) noexcept : ptr_(ptr) {
            if (ptr_ != nullptr)
                ptr_->retain();
        }

        RefCountPtr(const RefCountPtr& other) noexcept : ptr_(other.ptr_) {
            if (ptr_ != nullptr)
                ptr_->retain();
        }

        template <std::derived_from<T> U>
        explicit(false) RefCountPtr(const RefCountPtr<U>& other) noexcept : ptr_(other.get()) {
            if (ptr_ != nullptr)
                ptr_->retain();
        }

        RefCountPtr(RefCountPtr&& other) noexcept : ptr_(other.ptr_) {
            other.ptr_ = nullptr;
        }

        template <std::derived_from<T> U>
        explicit(false) RefCountPtr(RefCountPtr<U>&& other) noexcept : ptr_(other.release()) {
            other.ptr_ = nullptr;
        }

        ~RefCountPtr() noexcept {
            if (ptr_ != nullptr)
                ptr_->release();
        }

        RefCountPtr& operator=(const RefCountPtr& other) noexcept {
            if (this != std::addressof(other)) {
                reset(other.ptr_);
            }
            return *this;
        }

        template <std::derived_from<T> U>
        RefCountPtr& operator=(const RefCountPtr<U>& other) noexcept {
            reset(other.get());
            return *this;
        }

        RefCountPtr& operator==(RefCountPtr&& other) {
            if (this != std::addressof(other)) {
                if (ptr_) {
                    ptr_->release();
                }

                ptr_ = other.ptr_;
                other.ptr_ = nullptr;
            }
            return *this;
        }

        template <std::derived_from<T> U>
        RefCountPtr& operator==(RefCountPtr&& other) {
            if (ptr_) {
                ptr_->release();
            }

            ptr_ = other.ptr_;
            other.ptr_ = nullptr;

            return *this;
        }

        RefCountPtr& operator=(T* ptr) noexcept {
            reset(ptr);
            return *this;
        }

        [[nodiscard]] constexpr T* get() const noexcept {
            return ptr_;
        }

        [[nodiscard]] constexpr T* operator->() const noexcept {
            return ptr_;
        }

        [[nodiscard]] constexpr T& operator*() const noexcept {
            return *ptr_;
        }

        void reset() noexcept {
            if (ptr_ != nullptr) {
                ptr_->release();
                ptr_ = nullptr;
            }
        }

        void reset(T* ptr) noexcept {
            if (ptr == ptr_) return;

            if (ptr_ != nullptr) ptr_->release();

            ptr_ = ptr;
            if (ptr_ != nullptr) ptr_->retain();
        }

        void swap(RefCountPtr& other) noexcept {
            std::swap(ptr_, other.ptr_);
        }

        [[nodiscard]] friend bool operator==(const RefCountPtr& lhs, const RefCountPtr& rhs) noexcept {
            return lhs.ptr_ == rhs.ptr_;
        }

        template <std::derived_from<T> U>
        [[nodiscard]] friend bool operator==(const RefCountPtr& lhs, const RefCountPtr<U>& rhs) noexcept {
            return lhs.ptr_ == rhs.ptr_;
        }

        template <std::derived_from<T> U>
        [[nodiscard]] friend bool operator==(const RefCountPtr<U>& lhs, const RefCountPtr& rhs) noexcept {
            return lhs.ptr_ == rhs.ptr_;
        }

        [[nodiscard]] friend std::strong_ordering operator<=>(const RefCountPtr& lhs, const RefCountPtr& rhs) noexcept {
            return lhs.ptr_ <=> rhs.ptr_;
        }

        template <std::derived_from<T> U>
        [[nodiscard]] friend std::strong_ordering operator<=>(const RefCountPtr& lhs, const RefCountPtr<U>& rhs) noexcept {
            return lhs.ptr_ <=> rhs.ptr_;
        }

        template <std::derived_from<T> U>
        [[nodiscard]] friend std::strong_ordering operator<=>(const RefCountPtr<U>& lhs, const RefCountPtr& rhs) noexcept {
            return lhs.ptr_ <=> rhs.ptr_;
        }

        template <Char CharType, typename Traits>
        friend std::basic_ostream<CharType, Traits>& operator<<(std::basic_ostream<CharType, Traits>& stream,
                                                                 const RefCountPtr<T>& ptr) {
            return stream << ptr.get();
        }

    private:
        T* ptr_{nullptr};
    };

    export template <std::derived_from<RefCounted> T>
    void swap(RefCountPtr<T>& a, RefCountPtr<T>& b) noexcept {
        a.swap(b);
    }

    export template <std::derived_from<RefCounted> T, typename... Args>
        requires std::constructible_from<T, Args...>
    RefCountPtr<T> make_ref_counted(Args&&... args) {
        return RefCountPtr<T>{new T{std::forward<Args>(args)...}};
    }
}

template <std::derived_from<retro::RefCounted> T>
struct std::hash<retro::RefCountPtr<T>> {
    hash() = default;

    size_t operator()(const retro::RefCountPtr<T>& ptr) const noexcept {
        return std::hash<T*>{}(ptr.get());
    }
};