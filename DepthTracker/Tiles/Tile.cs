using System.Runtime.Serialization;
using System.Drawing;
using DepthTracker.Connection;

namespace DepthTracker.Tiles
{
    [DataContract]
    public class Tile
    {
        #region attr

        private bool _initialized;

        private bool _touch;
        [DataMember(Name = "touch")]
        public bool Touch
        {
            get
            {
                return _touch;
            }
            set
            {
                if (_touch != value)
                {
                    _touch = value;
                    //NotifyChanged();
                }
            }
        }

        private int _row;
        [DataMember(Name = "row")]
        public int Row
        {
            get { return _row; }
            set
            {
                if (_row != value)
                {
                    _row = value;
                    NotifyChanged();
                }
                NotifyChanged();
            }
        }

        private int _col;
        [DataMember(Name = "col")]
        public int Col
        {
            get { return _col; }
            set
            {
                if (_row != value)
                {
                    _col = value;
                    NotifyChanged();
                }
                NotifyChanged();
            }
        }

        public Rectangle Rectangle { get; set; }

        private PipeClient _pipeClient { get; set; }

        #endregion

        public Tile(int row, int col, PipeClient pipeClient)
        {
            _pipeClient = pipeClient;
            _row = row;
            _col = col;
            _initialized = true;
        }

        public void NotifyChanged()
        {
            if(_initialized)
                _pipeClient.SendJson(this.Serialize());
        }

        public bool Affected;
        public bool IsDirty;
        public void SetTouch(bool touch)
        {
            if (Affected)
                return;
            Affected = true;
            IsDirty = Touch == touch;
            Touch = touch;
        }

        public void SendData(PipeClient pipeClient)
        {
            if(IsDirty)
                pipeClient.SendJson(this.Serialize());
        }

        public bool Handled { get; set; }
    }
}
