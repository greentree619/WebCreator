using Google.Cloud.Firestore;

namespace WebCreator.Models
{
    public class ScrapProgress
    {
        public Int32 totalStep { get; set; } = 10;
        public Int32 curStep { get; set; } = 0;
        public Int32 progress { get; set; } = 0;
        public Boolean isComplete { get; set; } = false;

        public void InitProgress() {
            totalStep = 10;
            curStep = 0;
            progress = 0;
            isComplete = false;
        }

        public void SetProgress(int _totalStep, int _curStep, int _progress, bool _isComplete) {
            totalStep = _totalStep;
            curStep = _curStep;
            progress = _progress;
            isComplete = _isComplete;
        }

        public void MoveNextStep() {
            curStep++;
            progress = 0;
        }

        public void SetProgress(int _progress) {
            progress = _progress;
        }

        public void SetDone() {
            isComplete = true;
            progress = 100;
        }
    }
}
