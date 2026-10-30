EXTERNAL wait_for(eventId)
EXTERNAL wait_seconds(seconds)
EXTERNAL wait_all()
EXTERNAL event(eventId)
EXTERNAL set_presentation_mode(mode) // demo only, bound by DemoPresentationModeBinding

// ---------------------------------------------------
// --------------- ↓ actual dialogue ↓ ---------------

-> section1 // directs the dialogue to a section 



=== section1 === 

hii i'm One #speaker:alien1 
i'm an alien 
from outer space 

hi One #speaker:player 

hii i'm Two #speaker:alien2 
this is me but bigger and louder #volume:2 #size:2 
this is me talking at the same time as One #async 

i'm talking at the same time as Two #speaker:alien1  #async 

~ wait_all() 
// now alien1 and alien2 are both done 

~ wait_seconds(2.5)
// 2.5 seconds later... 

-> section2 // jumps to section2 




=== section2 === 

what should we do next? #speaker:alien1 
+ [Option A] -> optionA
+ [Option B] -> optionB
+ [Bye bye] -> bye




=== optionA === 

you clicked the "Option A" button! #speaker:alien1 

-> section2 // you can redirect back to a previously played section 




=== optionB === 

you clicked the "Option B" button! #speaker:alien2 

-> section2





=== bye ===

wait, you have to wave goodbye! #speaker:alien1 

~ wait_for("player_waved") 
// pauses here until the game informs Ink that "player_waved" occurred 

ok, now you can go #speaker:alien1 

-> END // tells the game this dialogue is over 

// --------------- ↑ actual dialogue ↑ ---------------
// ---------------------------------------------------

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

