namespace Sewer56.DeltaPatchGenerator.Lib.Utility
{
    public class Events
    {
        /// <summary>
        /// Defines a callback responsible for handling current progress.
        /// </summary>
        /// <param name="text">Text to display.</param>
        /// <param name="progress">Progress, scaled 0 to 1.</param>
        public delegate void ProgressCallback(string text, double progress);

        /// <summary>
        /// Allows you to optionally cancel the deletion of a file.
        /// </summary>
        /// <param name="relativePath">Relative path of the file to be deleted.</param>
        /// <returns>True to delete file, else false.</returns>
        public delegate bool ShouldDeleteFileCallback(string relativePath);
    }
}
