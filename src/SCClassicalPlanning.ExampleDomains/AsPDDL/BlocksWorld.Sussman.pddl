(define (problem sussman-anomaly)
    (:domain blocks-world)
    (:objects 
        blockA 
        blockB
        blockC
    )
    (:init
        (= Table Table)
        (Block blockA)
        (= blockA blockA)
        (Block blockB)
        (= blockB blockB)
        (Block blockC)
        (= blockC blockC)
        (On blockA Table)
        (On blockB Table)
        (On blockC blockA)
        (Clear blockB)
        (Clear blockC)
    )
    (:goal (and
            (On blockA blockB)
            (On blockB blockC)
        )
    )
)
