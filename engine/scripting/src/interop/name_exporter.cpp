//
// Created by fcors on 12/25/2025.
//
module retro.scripting;

using retro::FindType;
using retro::Name;

namespace retro::name_exporter
{
    void register_exported_functions()
    {
        const Name module_name = u"NameExporter";
        BindsManager::register_exported_function(module_name, ExportedFunction{u"Lookup", &lookup});
        BindsManager::register_exported_function(module_name, ExportedFunction{u"IsValid", &is_valid});
        BindsManager::register_exported_function(module_name, ExportedFunction{u"Equals", &equals});
        BindsManager::register_exported_function(module_name, ExportedFunction{u"ToString", &to_string});
    }

    Name lookup(const char16_t *name, const int32 length, const FindType find_type)
    {
        return Name{std::u16string_view{name, static_cast<usize>(length)}, find_type};
    }

    bool is_valid(const Name name)
    {
        return name.is_valid();
    }

    bool equals(const Name lhs, const char16_t *rhs, const int32 rhs_len)
    {
        return lhs == std::u16string_view{rhs, static_cast<usize>(rhs_len)};
    }

    int32 to_string(const Name name, char16_t *buffer, const int32 length)
    {
        const auto as_string = name.to_string();
        const usize string_length = std::min(as_string.size(), static_cast<usize>(length));
        std::memcpy(buffer, as_string.data(), string_length);
        return string_length;
    }

    static const Name MODULE_NAME = u"NameExporter";

} // namespace retro::name_exporter
