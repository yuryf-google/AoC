﻿namespace AoC21;

public class Day22Test
{
    enum Axis {X,Y,Z}
    static readonly Axis[] AxisList = { Axis.X, Axis.Y, Axis.Z };
    record Point( int X, int Y, int Z ) {
        public int Get( Axis a ) => a switch { Axis.X => X, Axis.Y => Y, Axis.Z => Z }; 
    }
    record Cube( Point Min, Point Max ) {
        public int Size(Axis a) {
            var r = Range(a);
            return r.Max - r.Min + 1;
        } 
        public long Count => AxisList.Aggregate( 1L, (a,e) => a * Size( e ) );
        public Range Range( Axis a ) => a switch {
            Axis.X => new Range( Min.X, Max.X ),
            Axis.Y => new Range( Min.Y, Max.Y ),
            Axis.Z => new Range( Min.Z, Max.Z )
        };
    }
    record Range( int Min, int Max ) {}

    static ( bool On, Cube Cube ) ParseRange( string line ) {
        // on x=-36..10,y=-44..1,z=-18..28
        var xyz = line.Split(',').Select(_ => _.Split('=')[1].Split("..").Select(int.Parse).ToArray()).ToArray();
        var cube = new Cube( new Point( xyz[0][0], xyz[1][0], xyz[2][0] ), 
            new Point( xyz[0][1], xyz[1][1], xyz[2][1] ) );
        var on = line.Contains( "on"  );
        return (on, cube);
    }

    static Range? Limit( Range r ) {
        const int Max = 50;
        if ( r.Min > Max || r.Max < -Max ) return null;
        Func<int,int> limit = (x) => x > Max ? Max : ( x < -Max ? -Max : x );
        return new Range( limit(r.Min), limit(r.Max) );
    }

    static Cube? Limit( Cube c ) {
        var x = Limit( new Range( c.Min.X, c.Max.X ) );
        var y = Limit( new Range( c.Min.Y, c.Max.Y ) );
        var z = Limit( new Range( c.Min.Z, c.Max.Z ) );
        if ( x == null || y == null || z == null ) return null;
        return new Cube( new Point( x.Min, y.Min, z.Min ), new Point( x.Max, y.Max, z.Max ) );
    }

    static IEnumerable<Point> RangePoints( Cube r ) 
        => Enumerable.Range( r.Min.X, r.Size( Axis.X ) )
            .SelectMany( x => Enumerable.Range( r.Min.Y, r.Size( Axis.Y ) )
            .SelectMany( y => Enumerable.Range( r.Min.Z, r.Size( Axis.Z ) )
            .Select( z => new Point( x, y, z ) ) ) );

    static bool Within( Range r, int x ) => x >= r.Min &&  x <= r.Max;
    static Range? Intersect( Range a, Range b ) {
        var isAOnLeft = a.Min <= b.Min;  // a starts on the left
        var a1 = isAOnLeft ? a : b;
        var b1 = isAOnLeft ? b : a;
        // {}   []
        // {[]}
        // {[}]
        if ( a1.Max >= b1.Min ) {
            // intersec
        } else return null;
        return new Range( b1.Min, Math.Min( a1.Max, b1.Max ) );
    }

    record RangeOn( Range Range, bool On ) {}

    static IEnumerable<RangeOn> Cut(RangeOn a, RangeOn b) {
        var i = Intersect( a.Range, b.Range ); // what is in b
        if ( i != null ) {
            var left = Math.Min( a.Range.Min, b.Range.Min );
            var right = Math.Max( a.Range.Max, b.Range.Max );
            var rl = new Range( left, i.Min - 1 );
            var rr = new Range( 1 + i.Max, right );
            var isBOnLeft = Within( b.Range, rl.Min );
            yield return new RangeOn( rl, isBOnLeft ? b.On : a.On );
            yield return new RangeOn( i, b.On );
            var isBOnRight = Within( b.Range, rr.Min );
            yield return new RangeOn(rr, isBOnRight ? b.On : a.On );
        }
    }

    static bool IsInside( Range big, Range small ) => big.Min <= small.Min && big.Max >= small.Max;

    static bool IsInside( Cube big, Cube small ) {
        if ( small.Count > big.Count ) return false; // does not fit
        return AxisList.All( a => IsInside( big.Range(a), small.Range(a) ) );
    }

    static bool IsNotTouching( Cube a, Cube b ) => AxisList.Any( ax => Intersect( a.Range(ax), b.Range(ax) ) == null );

    static IEnumerable<Cube> Split( Cube before, Cube newCube, bool newOn ) {
        if ( IsInside(before, newCube) ) {
            if ( newOn )
                // before is big enough and in the same state to inglobe the new state inside
                return new [] { before }; 
            
            // we need to make an off "hole" in the before cube
            
        }
        if ( IsNotTouching(before, newCube) )
            return newOn  ? new [] { before, newCube } // no issue they are not touching each other
                          : new [] { before }; // they are not touching and newCube is off

        Func<Axis,IEnumerable<RangeOn> > cut = (a) => Cut( 
            new RangeOn( new Range( before.Min.Get(a), before.Max.Get(a) ), true ), 
            new RangeOn( new Range( newCube.Min.Get(a), newCube.Max.Get(a) ), newOn ) )
            .Where( _ => _.Range.Min > _.Range.Max ) // only valid ranges
            .ToArray();

        var x = cut(Axis.X);
        var y = cut(Axis.Y);
        var z = cut(Axis.Z);

        var result = new List<Cube>();
        foreach ( var xi in x ) 
            foreach ( var yi in y )
                foreach ( var zi in z ) {
                    var on = xi.On && yi.On && zi.On;
                    result.Add( new Cube( new Point( xi.Range.Min, yi.Range.Min, zi.Range.Min ),
                        new Point( xi.Range.Max, yi.Range.Max, zi.Range.Max ) ) );
                }

        return result;
    }

    static void Execute1( bool on, Cube range, HashSet<Point> setOn ) {
        var limitedCube = Limit(range);
        if ( limitedCube == null ) return;
        Action<Point> aOn = (p) => setOn.Add(p);
        Action<Point> aOff = (p) => setOn.Remove(p);
        var a = on ? aOn : aOff;
        RangePoints(limitedCube).ToList().ForEach( a );
    }

    // 2,758,514,936,282,235
    // 9,223,372,036,854,775,807

    [TestCase("Day22/input.txt")]
    public async Task Test(string file) {
        var lines = await App.ReadLines(file);
        var instructions = lines.Select( ParseRange ).ToList();
        /*
        var setOn = new HashSet<Point>();
        instructions.ForEach( i => Execute1( i.On, i.Cube, setOn ) );
        setOn.Count().Should().Be(603661, "answer 1");
        */
        var regions = new [] { instructions.First() };
        foreach ( var i in instructions ) {
            // ??? when many small regions applied on a big one
            // if big contains entire small -> 
        }
        0.Should().Be(-2, "answer 2");
    }
}