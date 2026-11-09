EXTERNAL wait_for(eventId)
EXTERNAL wait_seconds(seconds)
EXTERNAL wait_all()
EXTERNAL event(eventId)

-> intro

=== intro ===
+ [START] 
    -> main

-> replay

=== replay ===
+ [REPLAY] 
    -> main

=== main ===
Hi! #speaker:Alien2

We heard your call. #speaker:Alien4 #async

Yeah, what’s happening? #speaker:Alien5 #async

Come over and meet my new friend! #speaker:Alien1

+ [Hi everyone!] 
    //emotion ~ awe, surprise
    WOW! You are so cool! #speaker:Alien5
    -> p1
+ [Ew, you guys are kinda ugly...] 
    //emotion ~ anger
    What are you talking about!? #speaker:Alien5
    -> p1
+ [You guys are so cute I can eat you] 
    //emotion ~ fear
    Eek! Don't eat us! Please! #speaker:Alien5
    -> p1
    
=== p1 ===

They are trying to find #speaker:Alien1
their "ship."

“Ship?” What is a ship? #speaker:Alien3

+ [A long white machine.] 
    -> p2
+ [You’ll know it when you see it.] 
    -> p2
+ [It’s very tall.] 
    -> p2

=== p2 ===
Any ideas on how to search? #speaker:Alien5

Does anyone see anything? #speaker:Alien2

//walk away
~ event("alien2_walk_away")
Is that going to work? #speaker:Alien3 #async 
~ wait_for("alien2_walk_away_complete")

I don't see anything here. #speaker:Alien2 

Say it again! We can't here you! #speaker:Alien5 #volume:2 #size:2.5

I don't see anything here. #speaker:Alien2 #volume 2 #size:2.5

Whaat? Say it again! #speaker:Alien5 #volume:3 #size:3

//emotion ~ anger, annoyed
Ugh, you guys are so annoying. #speaker:Alien2 #volume:3 #size:3.5
THERE IS NOTHING HERE, OK?!

//alien 2 walks back
~ event("alien2_walk_back")
~ wait_for("alien2_walk_back_complete")

We can try from up there? #speaker:Alien3

//everyone starts jumping
~ event("aliens_jump")
~ wait_seconds(2)

Everyone look up! #speaker:Alien4
I think I see something tall and long!

//everyone stops jumping 
~ event("aliens_stop_jump")

//emotion ~ depressed
No, that's just a large rock. #speaker:Alien2

//emotion ~ sadness, hopelessness
We're never going to find the ship... #speaker:Alien3
Should we just give up?

Woah woah everyone, calm down. #speaker:Alien1
THINK! #volume:2 #size:2.5
We have to help our friend get home.

//emotion ~ eager, happy
OH OH OH I KNOW! #speaker:Alien4 #volume:2 #size:2.5
Why don’t we use echolocation?

Yes, let’s try it! #speaker:Alien2

//use the soundwave effect
~ event("play_soundwave_vfx")
~ wait_seconds(2)

I think I’m hearing something. #speaker:Alien5

Yea, maybe a 30 minute walk?. #speaker:Alien1 #async

+ [Perfect, thank you!] 
    -> p3
+ [Took you guys long enough!] 
    -> p3
+ [Finally! I can go home now!] 
    -> p3

=== p3 ===

Haha, glad we could help! #speaker:Alien2 #async

Don’t be a stranger, come visit us! #speaker:Alien5

+ [Bye everyone!] 
    -> outro
+ [Smell ya later!] 
    -> outro
+ [Can’t wait to get out of here!] 
    -> outro
    
=== outro ===

See you around! #speaker:Alien1 #async

~ wait_seconds(3)

-> replay

=== function wait_for(eventId) ===
~ return

=== function wait_seconds(seconds) ===
~ return

=== function wait_all() ===
~ return

=== function event(eventId) ===
~ return
