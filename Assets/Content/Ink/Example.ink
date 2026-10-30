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

-> section2 // sections can direct to each other 

=== section2 === 

hii i'm Two #speaker:alien2 
this is me but bigger and louder #volume:2 #size:2 
this is me talking at the same time as One #async 

i'm talking at the same time as Two #speaker:alien1  #async 

what should we do? #speaker:alien1 
+ [Option A] -> optionA
+ [Option B] -> optionB

=== optionA === 

you clicked the "Option A" button! #speaker:alien1 

-> section2 // you can redirect back to a previously played section 

=== optionB === 

you clicked the "Option B" button! #speaker:alien2 

-> END // tells unity this dialogue is over 

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

