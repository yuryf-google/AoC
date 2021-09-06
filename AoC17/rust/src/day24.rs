extern crate regex;
use regex::Regex;

type Component = (i32,i32);
type Bridge = Vec<Component>;

fn parse( line : &str ) -> Component {
    lazy_static! { static ref RE_LINE: Regex = Regex::new( r"(\d+)/(\d+)$").unwrap(); }
    return RE_LINE.captures_iter(line).last()
        .map(|cap| {
            let a : i32 = cap[1].to_string().parse().unwrap();
            let b : i32 = cap[2].to_string().parse().unwrap();
            return ( a, b );
        } ).unwrap();
}

fn invert( c : Component ) -> Component { ( c.1, c.0 ) }

fn normalize( c : Component ) -> Component { if c.0 <= c.1 { c } else { invert( c ) } }

fn strength( bridge: Vec<Component> ) -> i32 { bridge.iter().map( |p| p.0 + p.1 ).sum() }

fn minus( components : Bridge, component : Component ) -> Bridge {
    let mut result = components.clone();
    result.remove(components.iter().position( |x| *x == component ).unwrap());
    result
}

fn max_strength( head : Bridge, components : Bridge ) -> i32 {
    let last : i32 = head.last().unwrap().1;
    let head_strength = strength(head);
    if components.len() == 0 { return head_strength; }
    let first: Vec<&Component> = components.iter().filter( |i| i.0 == last ).collect();
    let second : Vec<&Component> = components.iter().filter( |i| i.1 == last ).collect();
    let h1 = first.iter().map( |h|
        max_strength( [**h].to_vec(), minus(components.clone(), **h) ) )
        .max().unwrap_or(0);
    let h2 = second.iter().map( |h|
        max_strength( [ invert( **h )].to_vec(), minus(components.clone(), **h) ) )
        .max().unwrap_or(0);
    head_strength + std::cmp::max( h1, h2 )
}

pub fn task12( components : &str ) -> (i32,i32) {
    let c : Bridge = components.lines().map( |l| normalize(parse(l)) ).collect();
    let heads : Bridge = c.iter().filter( |a| a.0 == 0 ).map(|i| *i).collect();
    let a1 : i32 = heads.iter()
        .map( |h| max_strength( [*h].to_vec(), minus(c.clone(), *h) ) )
        .max().unwrap();
    return ( a1 ,-1); // 2612
}