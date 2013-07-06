// F# Twitter Feed Sample using Event processing
//

#load "show.fs"
#load "events.fs"
#load "stats.fs"
#load "TwitterPassword.fs"
#load "TwitterStream.fsx"





//----------------------------------------------
//

let twitterStream = 
    new TwitterStreamSample(userName, password)

twitterStream.NewTweet
   |> Event.add show 


twitterStream.StopListening()


//-----------------------------
// Tweet Parsing


twitterStream.NewTweet 
    |> Event.map parseTweet
    |> Event.add show

twitterStream.StopListening()

//-----------------------------
// Tweet Parsing (words)


twitterStream.NewTweet 
    // Parse the tweets (allows a map and filter at the same time)
    |> Event.choose parseTweet
    // Map the username and status (like Select in Linq)
    |> Event.map (fun tweet -> tweet.User.UserName, tweet.Status)
    |> Event.add show

twitterStream.StopListening()



//-----------------------------
// Word analysis

let words (s:string) = 
   s.Split([|' ' |], System.StringSplitOptions.RemoveEmptyEntries)

words "All the pretty horses" |> show



//-----------------------------
// Word counting


twitterStream.NewTweet
   // Parse the tweets (allows a map and filter at the same time)
   |> Event.choose parseTweet
   // Get the words in the status
   |> Event.map (fun x -> words x.Status)
   // Add the words to an incremental histogram
   |> Event.histogram
   // Visualize every 3 tweets
   |> Event.every 3
   // For each event, find the top 50 entries
   |> Event.map (Histogram.top 50)
   // Show
   |> Event.add show


twitterStream.StopListening()



//-----------------------------
// User Counting


twitterStream.NewTweet
   // Parse the tweets
   |> Event.choose parseTweet
   // Incrementally index by user name
   |> Event.indexBy (fun tweet -> tweet.User.UserName)
   // Visualize every 10 tweets
   |> Event.every 10
   // Find the current count and average/user
   |> Event.map (fun s -> 
         let avg = s |> Seq.averageBy (fun (KeyValue(_,d)) -> float d.Length)
         sprintf "#users = %d, avg tweets = %g" s.Count avg)
   // Show
   |> Event.add show

twitterStream.StopListening()



//-----------------------------
// Tweet indexing


twitterStream.NewTweet 
    |> Event.choose parseTweet
    // Builds up a Map of username and status from each tweet event (z is the accumulator)
    |> Event.scan (fun z x -> MultiMap.add x.User.UserName x.Status z) MultiMap.empty
    // Visualize every 10 tweets
    |> Event.every 10
    // Show
    |> Event.add show

twitterStream.StopListening()

