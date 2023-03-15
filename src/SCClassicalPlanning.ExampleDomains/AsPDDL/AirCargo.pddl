(define (domain air-cargo)
    (:action Load
        :parameters (?cargo ?plane ?airport)
        :precondition (and
            (Cargo ?cargo)
            (Plane ?plane)
            (Airport ?airport)
            (At ?cargo ?airport)
            (At ?plane ?airport)
        )
        :effect (and
            (not (At ?cargo ?airport))
            (In ?cargo ?plane)
        )
    )
    (:action Unload
        :parameters (?cargo ?plane ?airport)
        :precondition (and
            (Cargo ?cargo)
            (Plane ?plane)
            (Airport ?airport)
            (In ?cargo ?plane)
            (At ?plane ?airport)
        )
        :effect (and
            (At ?cargo ?airport)
            (not (In ?cargo ?plane))
        )
    )
    (:action Fly
        :parameters (?plane ?from ?to)
        :precondition (and
            (Plane ?plane)
            (Airport ?from)
            (Airport ?to)
            (At ?plane ?from)
        )
        :effect (and
            (not (At ?plane ?from))
            (At ?plane ?to)
        )
    )
)
