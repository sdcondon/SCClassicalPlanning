(define (domain blocks-world)

    (:constants Table)

    (:action MOVE
        :parameters (?block ?from ?toBlock)
        :precondition (and
            (block ?block)
            (block ?toBlock)
            (not (equal ?block ?from))
            (not (equal ?block ?toBlock))
            (not (equal ?from ?toBlock))
            (on ?block ?from)
            (clear ?block)
            (clear ?toBlock)
        )
        :effect (and
            (on ?block ?toBlock)
            (clear ?from)
            (not (on ?block ?from))
            (not (clear ?toBlock))
        )
    )

    (:action MOVE-TO-TABLE
        :parameters (?block ?from)
        :precondition (and
            (block ?block)
            (not (equal ?block ?from))
            (not (equal ?from Table))
            (on ?block ?from)
            (clear ?block)
        )
        :effect (and
            (On ?block Table)
            (Clear ?from)
            (not (on ?block ?from))
        )
    )
)
