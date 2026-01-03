//
// Created by fcors on 1/2/2026.
//
module retro.logging;

namespace retro
{
    std::shared_ptr<spdlog::logger> & default_logger_storage()
    {
        static std::shared_ptr<spdlog::logger> logger{};
        return logger;
    }

    std::shared_ptr<spdlog::logger> get_or_create_default_logger()
    {
        auto& storage = default_logger_storage();
        if (storage == nullptr)
        {
            storage = spdlog::stdout_color_mt("engine");
        }
        return storage;
    }

    void init_default_console(LogLevel level)
    {
        auto& storage = default_logger_storage();
        if (storage == nullptr)
        {
            storage = spdlog::stdout_color_mt("engine");
        }

        storage->set_level(to_spd_level(level));
        storage->set_pattern("[%Y-%m-%d %H:%M:%S.%e] [%^%l%$] [%n] %v");

        spdlog::set_default_logger(storage);
        spdlog::set_level(to_spd_level(level));
    }

    void set_default_logger(std::shared_ptr<spdlog::logger> logger)
    {
        default_logger_storage() = std::move(logger);
        spdlog::set_default_logger(default_logger_storage());
    }

    void shutdown_logging()
    {
        default_logger_storage().reset();
        spdlog::shutdown();
    }

    void set_level(const LogLevel level)
    {
        const auto logger = get_or_create_default_logger();
        logger->set_level(to_spd_level(level));
        spdlog::set_level(to_spd_level(level));
    }
}
