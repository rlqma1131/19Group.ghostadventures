namespace _01.Scripts.Object.BaseClasses.Interfaces
{
    public interface IPossessable {
        /// <summary>
        /// 이 오브젝트가 빙의 가능한 물체인지 판별하는 함수
        /// </summary>
        /// <returns></returns>
        public bool HasActivated();
        
        /// <summary>
        /// Called when the user tries to possess the object
        /// </summary>
        public virtual void TryPossess() { }
    
        /// <summary>
        /// Called when the user unpossesses from the object
        /// </summary>
        public virtual void Unpossess() { }
    
        /// <summary>
        /// Animation Event of entering possession state
        /// 필요에 따라 빙의애니메이션 끝나면 구현되는 기능들을 넣어주세요.
        /// </summary>
        public virtual void OnPossessionEnterComplete() { }
    
        /// <summary>
        /// Called when the user successes QTE Event
        /// </summary>
        public virtual void OnQTESuccess() { }

        /// <summary>
        /// Called when the user failed QTE Event 
        /// </summary>
        public void OnQTEFailure();
    }
}