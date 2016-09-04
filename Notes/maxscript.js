ro = rollout "Dialog für Eingabe"
(
    edittext edtInput "Eingabe:"  

    function PrintNumbers number =
    (
     print (number as string)
    )

    function DestroyDialog = destroydialog ro

    function FindAnswer numberToCheck isAnswer: notTheAnswer: =
    (
     if numberToCheck == 42 then
     (
         isAnswer()
     )
     notTheAnswer number
    )

    function ReadNumbersFromEdittext  = 
    (
        try
        (
            number = edtInput.text as integer
           return number
        )
        catch(throw "Not a Number!")
    )

    function Main =
    (
       try
        (
            number =   ReadNumbersFromEdittext() 
            FindAnswer number isAnswer:DestroyDialog notTheAnswer:PrintNumber

        )
        catch(Exception ex)
        (
           print ex 
        )
    ) 

    on edtInput entered arg do
    (
        Main()
    )
)

createdialog ro "Dialog für Konsoleneingabe"
  
   
