#include "common.h"

namespace day13 {

// list always first char [ and last ]
std::vector<std::string_view> SplitList(std::string_view list) noexcept {
  return {};  // TODO implement
}

std::optional<int> GetValue(std::string_view text) noexcept {
  int value{0};
  return absl::SimpleAtoi(text, &value) ? std::optional<int>(value)
                                        : std::optional<int>();
}

enum class Order { Right, Wrong, Continue };

// in all cases highest level is always a list
Order GetOrder(std::string_view l, std::string_view r) noexcept {
  const auto lo = GetValue(l);
  const auto ro = GetValue(r);

  if (lo.has_value() && ro.has_value()) {
    // simple case: 2 values to compare
    return lo.value() < ro.value()   ? Order::Right
           : lo.value() > ro.value() ? Order::Wrong
                                     : Order::Continue;
  }
  // all other cases we compare lists (but we could need to convert a value to a
  // list)
  const auto ll = SplitList(l);
  const auto rl = SplitList(r);
  const auto ls = ll.size();
  const auto rs = rl.size();
  // If the right list runs out of items first, the inputs are not in the right
  // order.
  if (rs < ls) return Order::Wrong;  // right list is shorter
  for (int i = 0; i < ls; i++) {
    const auto oi = GetOrder(ll[i], rl[i]);
    if (oi == Order::Continue) continue;
    return oi;
  }
  // If the left list runs out of items first, the inputs are in the right
  // order.
  return ls == rs ? Order::Continue : Order::Right;
}

int Answer1(std::string_view file) noexcept {
  const auto data = ReadData(file);
  int answer1 = 0;
  int index = 0;
  for (int i = 0; i < data.size() - 1; i++) {
    const auto& a = data[i];
    const auto& b = data[i + 1];
    if (!a.empty() && !b.empty()) {
      index++;
      answer1 += GetOrder(a, b) == Order::Right ? index : 0;
    }
  }
  return answer1;
}

}  // namespace day13

TEST(AoC22, Day13) {
  // basic tests
  EXPECT_EQ(day13::GetValue("13").value(), 13);
  EXPECT_FALSE(day13::GetValue("[1,2]").has_value());
  EXPECT_EQ(day13::GetOrder("13", "15"), day13::Order::Right);
  EXPECT_EQ(day13::GetOrder("20", "15"), day13::Order::Wrong);
  EXPECT_EQ(day13::GetOrder("1", "1"), day13::Order::Continue);

  const auto answer1 = day13::Answer1("13-sample");
  EXPECT_EQ(answer1, 13);
}
