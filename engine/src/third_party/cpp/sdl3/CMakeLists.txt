# @file CMakeLists.txt
#
# @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for full license information.
add_library(sdl3_cpp OBJECT)

target_sources(sdl3_cpp PUBLIC
        FILE_SET CXX_MODULES FILES
        sdl.ixx
        assert.ixx
        asyncio.ixx
        atomic.ixx
        audio.ixx
        bits.ixx
        blendmode.ixx
        callbackWrapper.ixx
        camera.ixx
        clipboard.ixx
        cpuinfo.ixx
        dialog.ixx
        endian.ixx
        error.ixx
        events.ixx
        filesystem.ixx
        gamepad.ixx
        gpu.ixx
        guid.ixx
        haptic.ixx
        hidapi.ixx
        hints.ixx
        image.ixx
        init.ixx
        intrin.ixx
        iostream.ixx
        joystick.ixx
        keyboard.ixx
        keycode.ixx
        loadso.ixx
        locale.ixx
        log.ixx
        main.ixx
        messagebox.ixx
        metal.ixx
        misc.ixx
        mixer.ixx
        mouse.ixx
        mutex.ixx
        net.ixx
        optionalRef.ixx
        ownPtr.ixx
        pen.ixx
        pixels.ixx
        platform.ixx
        power.ixx
        process.ixx
        rect.ixx
        render.ixx
        resource.ixx
        scancode.ixx
        sensor.ixx
        spanRef.ixx
        stdinc.ixx
        storage.ixx
        strings.ixx
        surface.ixx
        system.ixx
        thread.ixx
        time.ixx
        timer.ixx
        touch.ixx
        tray.ixx
        ttf.ixx
        video.ixx
        vulkan.ixx)

find_package(SDL3 REQUIRED)
target_link_libraries(sdl3_cpp PUBLIC SDL3::SDL3)

add_library(SDL3::Cpp ALIAS sdl3_cpp)

set_target_properties(sdl3_cpp PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${RETRO_BIN_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${RETRO_LIB_DIR}")
