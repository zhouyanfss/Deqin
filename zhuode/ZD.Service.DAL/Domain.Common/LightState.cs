using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD.Service.DAL.Domain.Common
{
    public class LightStateAction
    {
        public string ActionName { get; set; }

        public object ActionData { get; set; }
    }

    public interface ILightState
    {
        void Change(string actionName, LightStateContext context);

        void Action(string actionName, object actionData);
    }

    public class LightStateContext
    {
        private ILightState _state;

        public LightStateContext(ILightState state)
        {
            _state = state;
        }

        public void ChangeState(ILightState state)
        {
            _state = state;
        }

        public virtual void Action(LightStateAction request)
        {
            _state.Change(request.ActionName, this);

            _state.Action(request.ActionName, request.ActionData);
        }
    }

    public abstract class StateBase : ILightState
    {
        public virtual void Change(string actionName, LightStateContext context)
        {
            if (ActionTrans.ContainsKey(actionName))
            {
                context.ChangeState(ActionTrans[actionName]);
            }
            else if (ShouldRemain(actionName))
            {
                // keep the state
            }
            else
            {
                throw new ArgumentException(string.Format("Action:{0} cannot be re recognised", actionName));
            }
        }

        protected virtual bool ShouldRemain(string actionName)
        {
            return false;
        }

        private IDictionary<string, ILightState> _actionTrans = null;
        public IDictionary<string, ILightState> ActionTrans
        {
            get
            {
                if (_actionTrans == null)
                {
                    _actionTrans = new Dictionary<string, ILightState>();
                    InitActionTrans(_actionTrans);
                }

                return _actionTrans;
            }
        }

        protected virtual void InitActionTrans(IDictionary<string, ILightState> actionTrans)
        {

        }

        public virtual void Action(string actionName, object actionData)
        {

        }
    }
}
