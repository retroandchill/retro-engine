//
// Created by fcors on 12/25/2025.
//
module retro.scripting;

namespace retro
{
    BindsManager &BindsManager::instance()
    {
        static BindsManager instance;
        return instance;
    }

    // NOLINTNEXTLINE
    void BindsManager::register_exported_function(const Name module_name, const ExportedFunction &exported_function)
    {
        auto &instance = BindsManager::instance();
        instance.exported_functions_[module_name].push_back(exported_function);
    }

    // NOLINTNEXTLINE
    void *BindsManager::get_bound_function(const char16_t *module_name,
                                           const int32 module_length,
                                           const char16_t *function_name,
                                           const int32 function_length,
                                           int32 param_size)
    {
        const Name name{std::u16string_view{module_name, static_cast<usize>(module_length)}, FindType::Find};
        if (name.is_none())
        {
            return nullptr;
        }

        const Name function{std::u16string_view{function_name, static_cast<usize>(function_length)}, FindType::Find};
        if (function.is_none())
        {
            return nullptr;
        }

        for (auto &instance = BindsManager::instance();
             const auto &exported_function : instance.exported_functions_[name])
        {
            if (exported_function.name == function)
            {
                return exported_function.function_ptr;
            }
        }

        return nullptr;
    }
} // namespace retro
