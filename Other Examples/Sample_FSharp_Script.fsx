open System
// example of higher order functions
let standardAlgorithm() = 2 * 2
let customAlgorithm() = 2 * 3    
let applyAlgorithm someNumber algorithm =
    someNumber * algorithm() 
do printf "Standard algorithm result is %O." 
    (applyAlgorithm 2 standardAlgorithm)
do printf "\r\nCustom algorithm result is %O." 
    (applyAlgorithm 2 customAlgorithm)
