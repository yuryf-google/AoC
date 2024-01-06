package day23

import (
	"github.com/yury-fedorov/AoC/AoC23/aoc"
	"slices"
	"strconv"
	"strings"
)

type Day23 struct{}

type Point struct{ x, y int16 }

var shifts = []Point{{0, 1}, {0, -1}, {1, 0}, {-1, 0}}

const Forrest = '#'
const Path = '.'
const Slopes = "v^><" // same sequence as shifts

var m = aoc.ReadFile("23-1")
var x0 = int16(slices.Index([]rune(m[0]), Path))
var yMax = int16(len(m) - 1)
var xMax = int16(len(m[0]) - 1)
var xEnd = int16(slices.Index([]rune(m[yMax]), Path))
var pStart = Point{x0, 0}
var pEnd = Point{xEnd, yMax}

func at(p Point) rune {
	if p.x < 0 || p.y < 0 || p.y > yMax || p.x > xMax {
		return Forrest
	}
	return ([]rune(m[p.y]))[p.x]
}

func checkSlope(material rune) (bool, Point) {
	i := strings.Index(Slopes, string(material))
	isSlope := i >= 0
	p := Point{}
	if isSlope {
		p = shifts[i]
	}
	return isSlope, p
}

func shift(p, s Point) Point { return Point{x: p.x + s.x, y: p.y + s.y} }

func next(p0 Point, part aoc.Part) []Point {
	material := at(p0)
	if part == aoc.Part1 {
		isSlope, slopeShift := checkSlope(material)
		if isSlope {
			return []Point{shift(p0, slopeShift)}
		}
	}
	var result []Point
	for _, s := range shifts {
		p1 := shift(p0, s)
		material = at(p1)
		if material != Forrest {
			result = append(result, p1)
		}
	}
	return result
}

func paths(path0 []Point, part aoc.Part) [][]Point {
	n := len(path0)
	lastPoint := path0[n-1]
	nn := next(lastPoint, part)
	var result [][]Point
	for _, n := range nn {
		if slices.Contains(path0, n) {
			continue
		}
		path1 := append(path0, n)
		if n == pEnd {
			result = append(result, path1)
		} else {
			result = append(result, paths(path1, part)...)
		}
	}
	return result
}

func solution(part aoc.Part) int {
	allPaths := paths([]Point{pStart}, part)
	var result int
	for _, pi := range allPaths {
		result = aoc.Max(result, len(pi)-1) // the starting point is not counted
	}
	return result
}

func (day Day23) Solve() aoc.Solution {
	return aoc.Solution{strconv.Itoa(solution(aoc.Part1)), strconv.Itoa(solution(aoc.Part2))}
}
