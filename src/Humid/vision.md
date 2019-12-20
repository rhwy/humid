# Long term

This library should always work as one simple file that you can add on any project. 
It should always keep a reasonable size with always just good enough code to make things work
It should have enough valuable tests, acting mostly as documentation on how it works



## Releases

### Next

One big difference with regular web frameworks would be the possibility of being dynamic in terms of routes. 
In order to do that, I would like to be able to dynamically load routes and implementations :
* by dropping a dll within a specific folder ?
    * need to test if libs used as dependencies are effectively used if in the same folder as this lib
* or by compiling on the fly new file on a folder containing routing code ?
    * in that case how are dependencies managed ?
    
 in all the cases, we need to :
 * be able to unload the same way we load 
 * manage versions if existing lib/routes already exist
 * report loaded routes