(define (problem sussman-anomaly)
    (:domain blocks-world)
    (:objects 
        blockA 
        blockB
        blockC
    )
    (:init
        (equal Table Table)
        (block blockA)
        (equal blockA blockA)
        (block blockB)
        (equal blockB blockB)
        (block blockC)
        (equal blockC blockC)
        (on blockA Table)
        (on blockB Table)
        (on blockC blockA)
        (clear blockB)
        (clear blockC)
    )
    (:goal (and
            (on blockA blockB)
            (on blockB blockC)
        )
    )
)
