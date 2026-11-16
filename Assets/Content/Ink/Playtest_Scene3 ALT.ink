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

So…any idea how to escape? #speaker:Alien2

<fear>Are we stuck here forever?</fear> #speaker:Alien4 #async

<fear>WE'RE ALL GOING TO DIE</fear> #speaker:Alien5 #async

<anger>This is your fault!</anger> #speaker:Alien1

+ [Chill out guys] 
    You're right, calm down everyone. #speaker:Alien5
    -> p1
+ [OMG OMG OMG] 
    //emotion ~ anger
    <anger>I have a family!?</anger> #speaker:Alien5
    -> p1
+ [I wanna stay here forever] 
    //emotion ~ fear
    Ok, weird sentiment... #speaker:Alien5
    -> p1
    
=== p1 ===

Wait everyone, it's going to be ok. #speaker:Alien3
<joy>Let's think of some ideas</joy>

Does anyone see anything? #speaker:Alien2

//walk away
~ event("alien2_walk_away")
Find an escpae route yet? #speaker:Alien3 #async 
~ wait_for("alien2_walk_away_complete")

<fear>It's so dark</fear> #speaker:Alien2 
I can't see anything.

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
~ wait_seconds(2)

Everyone look up! #speaker:Alien4
What is that?

//everyone stops jumping 
~ event("aliens_stop_jump")

//emotion ~ depressed
<sadness>Well, its not anything helpful.</sadness> #speaker:Alien2

//emotion ~ sadness, hopelessness
<fear>We're never going to find the ship...</fear> #speaker:Alien3

//emotion ~ eager, happy
<joy>Why don’t we use echolocation?</joyy> #speaker:Alien4

//use the soundwave effect
~ event("play_soundwave_vfx")
~ wait_seconds(3)

<sadness>I don't here anything...</sadness> #speaker:Alien5

<sadness>Yea, I'm not sure what else to do...</sadness> #speaker:Alien1 #async

+ [You guys are so useless.] 
    -> outro
+ [What the hell!] 
    -> outro
+ [Does this mean we can’t go home?] 
    -> outro
    
=== outro ===

<fear>EEK! Don't scream!</fear> #speaker:Alien2 #async 

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
