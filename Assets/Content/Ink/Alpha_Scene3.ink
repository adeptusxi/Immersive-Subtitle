EXTERNAL wait_for(eventId)
EXTERNAL wait_seconds(seconds)
EXTERNAL wait_all()
EXTERNAL event(eventId)

-> main

=== main ===
Hi! #speaker:Alien2

Hey, what’s up! #speaker:Alien3

I heard you call, #speaker:Alien4
what’s happening?

Yeah, what’s happening? #speaker:Alien5

I met a strange creature today. #speaker:Alien1
Come over and meet them!

* Hi!
    Hi everyone! #speaker:player
    Hi! #speaker:Alien5
    -> p1
* 'Sup?
    What’s up guys! #speaker:player 
    The sky! #speaker:Alien5
    -> p1
* Help!
    Please help me! #speaker:player
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

* A white machine
    It’s a long white machine. #speaker:player
    -> p2
* You’ll just know
    You’ll know it when you see it. #speaker:player
    -> p2
* A tall machine
    It’s very tall so it#speaker:player
    shouldn’t be hard to find. 
    -> p2

=== p2 ===

Interesting #speaker:Alien2

Any ideas on how to search? #speaker:Alien5

Let’s see if we can see #speaker:Alien2
it in the distance first.

//walk away

I don't see anything here. #speaker:Alien2 #volume:2 #size:2.5

Come back!. #speaker:Alien5 #volume:2 #size:2.5
That's not going to work. #volume:2 #size:2.5

//alien 2 walks back

I think we can try jumping. #speaker:Alien3
The rocks should give us
a higher view.

//everyone starts jumping
   
I can’t see anything! #speaker:Alien2

Wait, I think I see something #speaker:Alien4

What do you see? #speaker:Alien1

I see something tall and long #speaker:Alien4

Wait, wait, I want to see too! #speaker:Alien2

//jumps over

No, that can’t be right. #speaker:Alien2
I think that’s just a large rock.
Does anyone else see anything?

There’s something over here! #speaker:Alien3
Over there, within the clouds.

I don't see anything... #speaker:Alien5
are you sure? 

Why are you always questioning me? #speaker:Alien3
Do you not trust me?

Well... I just think that sometimes #Alien5
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

I think I’m hearing something. #speaker:Alien5

Yea, I think it’s over there! #speaker:Alien3
A bit towards the North.

Yes, I feel it too. #speaker:Alien1
Maybe a 30 minute walk?

* Perfect!
    Perfect, thank you #speaker:player
    so much for your help! 
    -> p3
* Finally...
    Took you guys long enough! #speaker:player
    -> p3
* Finally!
    Finally! I can go home now! #speaker:player
    -> p3

=== p3 ===

Haha, glad we could help! #speaker:Alien2

It was nice meeting you! #speaker:Alien4

Don’t be a stranger, come visit us! #speaker:Alien5

* Bye!
    Bye everyone! See you next time. #speaker:player
    -> outro
* Smell ya later!
    Smell ya later! #speaker:player
    -> outro
* Get out of here!
    Can’t wait to get out of here! #speaker:player
    -> outro
    
    
=== outro ===

See you around! #speaker:Alien1

We'll miss you! #speaker:Alien3

-> END

=== function wait_for(eventId) ===
~ return

=== function wait_seconds(seconds) ===
~ return

=== function wait_all() ===
~ return

=== function event(eventId) ===
~ return
