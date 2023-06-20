namespace TaoTie
{
    public enum TransitionInterruptionSource
    {
        /// <summary>
        ///   <para>The Transition cannot be interrupted. Formely know as Atomic.</para>
        /// </summary>
        None,
        /// <summary>
        ///   <para>The Transition can be interrupted by transitions in the source AnimatorState.</para>
        /// </summary>
        Source,
        /// <summary>
        ///   <para>The Transition can be interrupted by transitions in the destination AnimatorState.</para>
        /// </summary>
        Destination,
        /// <summary>
        ///   <para>The Transition can be interrupted by transitions in the source or the destination AnimatorState.</para>
        /// </summary>
        SourceThenDestination,
        /// <summary>
        ///   <para>The Transition can be interrupted by transitions in the source or the destination AnimatorState.</para>
        /// </summary>
        DestinationThenSource,
    }
}