#pragma once

#include <algorithm>
#include <fstream>
#include <numeric>
#include <string>
#include <string_view>
#include <vector>

#include "absl/strings/str_cat.h"
#include "absl/strings/str_split.h"
#include "gtest/gtest.h"

// to filter out slow tests
[[nodiscard]] inline bool IsFastOnly() noexcept { return true; }

[[nodiscard]] inline constexpr int Sign(auto x) noexcept {
  return x > 0 ? 1 : x < 0 ? -1 : 0;
}

[[nodiscard]] std::vector<std::string> ReadData(std::string_view file) noexcept;
