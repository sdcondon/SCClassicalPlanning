(define (problem two-planes)
    (:domain air-cargo)
    (:objects 
        cargo1
        cargo2
        plane1
        plane2
        airport1
        airport2
    )
    (:init
        (Cargo cargo1)
        (Cargo cargo2)
        (Plane plane1)
        (Plane plane2)
        (Airport airport1)
        (= airport1 airport1)
        (Airport airport2)
        (= airport2 airport2)
        (At cargo1 airport1)
        (At cargo2 airport2)
        (At plane1 airport1)
        (At plane2 airport2)
    )
    (:goal (and
            (At cargo2 airport1)
            (At cargo1 airport2)
        )
    )
)
