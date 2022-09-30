namespace SCClassicalPlanningAlternatives.OwnFolModel.SentenceManipulation
{
    /// <summary>
    /// Interface for transformations of <see cref="State"/> instances.
    /// <para/>
    /// NB: Essentially an interface for visitors with a return value.
    /// </summary>
    /// <typeparam name="TOut">The type that the transformation transforms the sentence to.</typeparam>
    public interface ISentenceTransformation<out TOut>
    {

       ///// <summary>
       ///// Applies the transformation to an <see cref="Implication"/> instance. 
       ///// </summary>
       ///// <param name="implication">The <see cref="Implication"/> instance to transform.</param>
       //TOut ApplyTo(Implication implication);
       //
       ///// <summary>
       ///// Applies the transformation to a <see cref="Negation"/> instance. 
       ///// </summary>
       ///// <param name="negation">The <see cref="Negation"/> instance to transform.</param>
       //TOut ApplyTo(Negation negation);
       //
       ///// <summary>
       ///// Applies the transformation to a <see cref="Predicate"/> instance. 
       ///// </summary>
       ///// <param name="predicate">The <see cref="Predicate"/> instance to transform.</param>
       //TOut ApplyTo(Predicate predicate);
       //
       ///// <summary>
       ///// Applies the transformation to a <see cref="UniversalQuantification"/> instance. 
       ///// </summary>
       ///// <param name="universalQuantification">The <see cref="UniversalQuantification"/> instance to transform.</param>
       //TOut ApplyTo(UniversalQuantification universalQuantification);
    }
}
