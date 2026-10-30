EXTERNAL wait_for(eventId)
EXTERNAL wait_seconds(seconds)
EXTERNAL wait_all()
EXTERNAL event(eventId)
EXTERNAL set_presentation_mode(mode) // demo only, bound by DemoPresentationModeBinding

-> menu

=== menu ===
+ [Above Speaker]
    ~ set_presentation_mode("SpeakerAboveHead")
    -> intro
+ [Beside Speaker]
    ~ set_presentation_mode("SpeakerBesideHead")
    -> intro
+ [Halo]
    ~ set_presentation_mode("SpeakerHalo")
    -> intro

=== intro ===

~ wait_seconds(2)

Hey, do you see that? #speaker:Alien1

See what? #speaker:Alien2

The weird spiky thing, #speaker:Alien1
in the distance. 
I’ve never seen anything 
like it before.

Hmmm, #speaker:Alien2
I'm not sure either.
Do you think it can talk?

Yea, maybe. #speaker:Alien1
Should we go try it out?

Let’s yell from here. #speaker:Alien1

Hi! #speaker:Alien2 #volume:2 #size:2.5 

Hello! #speaker:Alien1 #volume:2 #size:2.5 #async

Hiiiiiiiiii! #speaker:Alien2 #volume:2 #size:2.5 #async 

Can you hear us? #speaker:Alien1 #volume:2 #size:2.5 #async

~ wait_seconds(3)

I don’t think it can hear us. #speaker:Alien2

Technically, #speaker:Alien1 
sound can’t travel in space
because there is no atmosphere.

Then… how are we talking #speaker:Alien2
to each other?

~ wait_seconds(2)

-> menu

-> END

=== function wait_for(eventId) ===
~ return

=== function wait_seconds(seconds) ===
~ return

=== function wait_all() ===
~ return

=== function event(eventId) ===
~ return

=== function set_presentation_mode(mode) ===
~ return

