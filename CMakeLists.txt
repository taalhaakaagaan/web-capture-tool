cmake_minimum_required(VERSION 3.15)
project(MultiLanguageProject)

# C++ configuration
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Add the executable
add_executable(cpp_app src/cpp/main.cpp)

# Link with WinINet and GDI+
if(WIN32)
    target_link_libraries(cpp_app PRIVATE wininet gdiplus shlwapi)
endif()

# Output directories
set_target_properties(cpp_app PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin")
