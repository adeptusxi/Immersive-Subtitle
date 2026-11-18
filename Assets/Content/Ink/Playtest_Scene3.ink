EXTERNAL wait_for(eventId)
EXTERNAL wait_seconds(seconds)
EXTERNAL wait_all()
EXTERNAL event(eventId)

-> intro

=== intro ===
+ [START] 
    -> main
+ [EMOTION TEST]
    -> emotions_demo

-> replay

=== replay ===
+ [REPLAY] 
    -> main

=== main ===

Hi! #speaker:Alien2

We heard your call. #speaker:Alien4

Yeah, what’s happening? #speaker:Alien5 #async

Come over and meet my new friend! #speaker:Alien1

+ [Hi everyone!] 
    //emotion ~ awe, surprise
    <joy>WOW! You are so cool!</joy> #speaker:Alien5
    -> p1
+ [Ew, you guys are kinda ugly...] 
    //emotion ~ anger
    <anger>What are you talking about!?</anger> #speaker:Alien5
    -> p1
+ [You guys are so cute I can eat you] 
    //emotion ~ fear
    <fear>Eek! Don't eat us! Please!</fear> #speaker:Alien5
    -> p1
    
=== p1 ===

They are trying to find #speaker:Alien1
their "ship."

Does anyone see anything? #speaker:Alien2
I'll check here.

//walk away
~ event("alien2_walk_away")
Is that going to work? #speaker:Alien3 #async 
~ wait_for("alien2_walk_away_complete")

I don't see anything here. #speaker:Alien2 

Say it again!#speaker:Alien5 #volume:2 #size:2.5
We can't hear you! #volume:2 #size:2.5

I don't see anything here!#speaker:Alien2 #volume:2 #size:2.5

Whaat? Say it again! #speaker:Alien5 #volume:3 #size:3

//emotion ~ anger, annoyed
Ugh, you guys#speaker:Alien2 #volume:3 #size:3.5
are <anger>so annoying.</anger>#volume:3 #size:3.5
<anger>THERE IS NOTHING</anger> #volume:3 #size:3.5
<anger>HERE, OK?!<anger> #volume:3 #size:3.5

//alien 2 walks back
~ event("alien2_walk_back")
~ wait_for("alien2_walk_back_complete")

We can try from up there? #speaker:Alien3

//everyone starts jumping
~ event("aliens_jump")
~ event("show_large_rock")
~ wait_seconds(2)

Everyone look up! #speaker:Alien4

//everyone stops jumping 
~ event("aliens_stop_jump")

//emotion ~ depressed
<sadness>No, that's just a large rock.</sadness> #speaker:Alien2

//emotion ~ sadness, hopelessness
<fear>We're never going to find the ship...</fear> #speaker:Alien3

//emotion ~ eager, happy
<joy>Why don’t we use echolocation?</joyy> #speaker:Alien4

//use the soundwave effect
~ event("play_soundwave_vfx")
~ event("show_spaceship")
~ wait_seconds(3)

I think I’m hearing something. #speaker:Alien5

Yea, maybe a 30 minute walk? #speaker:Alien1 #async

Haha, glad we could help! #speaker:Alien2 #async

+ [Bye everyone!] 
    -> outro
+ [Smell ya later!] 
    -> outro
+ [Can’t wait to get out of here!] 
    -> outro
    
=== outro ===

See you around! #speaker:Alien1 #async 

~ wait_seconds(2)

-> replay

=== emotions_demo ===

how is everyone feeling today? #speaker:alien1

~ wait_seconds(0.5)

i'm feeling <joy>so happy</joy> today! 

~ wait_seconds(0.5)

i'm feeling <sadness>a little sad</sadness> #speaker:alien3

~ wait_seconds(0.5)

i'm feeling <anger>really angry</anger> #speaker:alien4

~ wait_seconds(0.5)

and i'm feeling <fear>very scared</fear> #speaker:alien5

-> intro

=== function wait_for(eventId) ===
~ return

=== function wait_seconds(seconds) ===
~ return

=== function wait_all() ===
~ return

=== function event(eventId) ===
~ return
