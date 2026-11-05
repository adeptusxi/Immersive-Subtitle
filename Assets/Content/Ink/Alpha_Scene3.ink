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

Hey, what’s up! #speaker:Alien3

I heard you call, #speaker:Alien4
what’s happening?

Yeah, what’s happening? #speaker:Alien5

I met a strange creature today. #speaker:Alien1
Come over and meet them!

+ [Hi everyone!] 
    Hi! #speaker:Alien5
    -> p1
+ [What’s up guys!] 
    The sky! #speaker:Alien5
    -> p1
+ [Please help me!] 
    We'll try out best! #speaker:Alien5
    -> p1
    
=== p1 ===

They are lost #speaker:Alien1
and are trying to 
find their “ship” 

“Ship?” What is a ship? #speaker:Alien3

Can you eat it? #speaker:Alien2

Of course you can’t #speaker:Alien1
eat it silly! It is supposed to 
help them get home.

Home is important! #speaker:Alien4
We’ll help you find it.
How does it look like? 
What should we look for?

+ [A long white machine.] 
    -> p2
+ [You’ll know it when you see it.] 
    -> p2
+ [It’s very tall.] 
    -> p2

=== p2 ===

Interesting #speaker:Alien2

Any ideas on how to search? #speaker:Alien5

Let’s see if we can see #speaker:Alien2
it in the distance first.

//walk away
~ event("aliens_walk_away")
//~ wait_for("aliens_walk_away_complete") 

I don't see anything here. #speaker:Alien2 #volume:2 #size:2.5

Come back! #speaker:Alien5 #volume:2 #size:2.5
That's not going to work. #volume:2 #size:2.5

//alien 2 walks back
~ event("aliens_walk_back")
//~ wait_for("aliens_walk_back_complete") 

I think we can try jumping. #speaker:Alien3
The rocks should give us
a higher view.

//everyone starts jumping
~ event("aliens_jump")
~ wait_seconds(2)
   
I can’t see anything! #speaker:Alien2

Wait, I think I see something #speaker:Alien4

What do you see? #speaker:Alien1

I see something tall and long #speaker:Alien4

Wait, wait, I want to see too! #speaker:Alien2

No, that can’t be right. #speaker:Alien2
I think that’s just a large rock.
Does anyone else see anything?

//everyone stops jumping 
~ event("aliens_stop_jump")

There’s something over here! #speaker:Alien3
Over there, within the clouds.

I don't see anything... #speaker:Alien5
are you sure? 

Why are you always questioning me? #speaker:Alien3
Do you not trust me?

Well... I just think that sometimes #speaker:Alien5
you see things that don’t exist...

You little... #speaker:Alien3 #size:0.75
I’m gonna show you a new one!

Woah woah everyone, calm down. #speaker:Alien1
We need to stop fighting 
and find the ship! THINK! 
We have to help our friend get home.

Hmmm, why don’t we use echolocation? #speaker:Alien4
Let’s use our horns 
and see if anything bounces off them.

Yes, let’s try it! #speaker:Alien2

//use the soundwave effect
~ event("play_soundwave_vfx")
~ wait_seconds(2)

I think I’m hearing something. #speaker:Alien5

Yea, I think it’s over there! #speaker:Alien3
A bit towards the North.

Yes, I feel it too. #speaker:Alien1
Maybe a 30 minute walk?

+ [Perfect, thank you!] 
    -> p3
+ [Took you guys long enough!] 
    -> p3
+ [Finally! I can go home now!] 
    -> p3

=== p3 ===

Haha, glad we could help! #speaker:Alien2

It was nice meeting you! #speaker:Alien4

Don’t be a stranger, come visit us! #speaker:Alien5

+ [Bye everyone!] 
    -> outro
+ [Smell ya later!] 
    -> outro
+ [Can’t wait to get out of here!] 
    -> outro
    
=== outro ===

See you around! #speaker:Alien1

We'll miss you! #speaker:Alien3

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
