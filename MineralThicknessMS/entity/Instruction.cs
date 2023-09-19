using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace MineralThicknessMS.entity
{
    public class Instruction
    {
        private String SonarPositionSelfCheckL = "$SonarPositionSelfCheckR\r\n";//左水位自检测
        private String MoveSonarPositionToSelfCheckL = "$MoveSonarPositionToSelfCheckR\r\n";//左测深仪移动至自检位置
        private String SonarPositionMoveUpL = "$SonarPositionMoveUpR:\r\n";//左向上移动*秒
        private String SonarPositionMoveDownL = "$SonarPositionMoveDownR:\r\n";//左向下移动*秒
        private String SonarPositionMaintainL = "$SonarPositionMaintainR:\r\n";//左测杆自动调整

        private String SonarPositionSelfCheckR = "$SonarPositionSelfCheckL\r\n";//右水位自检测
        private String MoveSonarPositionToSelfCheckR = "$MoveSonarPositionToSelfCheckL\r\n";//右测深仪移动至自检位置
        private String SonarPositionMoveUpR = "$SonarPositionMoveUpL:\r\n";//右向上移动*秒
        private String SonarPositionMoveDownR = "$SonarPositionMoveDownL:\r\n";//右向下移动*秒
        private String SonarPositionMaintainR = "$SonarPositionMaintainL:\r\n";//右测杆自动调整

        private String BracketLMoveUp = "$BracketRMoveUp\r\n";  //左侧支架向上伸展
        private String BracketLMoveDown = "$BracketRMoveDown\r\n";  //左侧支架向下伸展
        private String BracketLMoveStop = "$BracketRMoveStop\r\n";  //左侧支架关闭

        private String BracketRMoveUp = "$BracketLMoveUp\r\n";  //右侧支架向上伸展
        private String BracketRMoveDown = "$BracketLMoveDown\r\n";  //右侧支架向下伸展
        private String BracketRMoveStop = "$BracketLMoveStop\r\n";  //右侧支架关闭

        private String TransducerLMoveUp = "$TransducerRMoveUp\r\n";    //左侧换能器向上伸展
        private String TransducerLMoveDown = "$TransducerRMoveDown\r\n";    //左侧换能器向下伸展
        private String TransducerLMoveStop = "$TransducerRMoveStop\r\n";    //左侧换能器关闭

        private String TransducerRMoveUp = "$TransducerLMoveUp\r\n";    //右侧换能器向上伸展
        private String TransducerRMoveDown = "$TransducerLMoveDown\r\n";    //右侧换能器向下伸展
        private String TransducerRMoveStop = "$TransducerLMoveStop\r\n";    //右侧换能器关闭

        private String StartWashingL = "$StartWashingR\r\n";    //左侧冲洗装置启动
        private String StopWashingL = "$StopWashingR\r\n";  //左侧冲洗装置关闭
        private String StartWashingR = "$StartWashingL\r\n";    //右侧冲洗装置启动
        private String StopWashingR = "$StopWashingL\r\n";  //右侧冲洗装置关闭

        private String StartTankHeatingL = "$StartTankHeatingR\r\n";    //左侧加热启动
        private String StopTankHeatingL = "$StopTankHeatingR\r\n";  //左侧加热关闭
        private String StartTankHeatingR = "$StartTankHeatingL\r\n";    //右侧加热启动
        private String StopTankHeatingR = "$StopTankHeatingL\r\n";//右侧加热关闭

        //右
        public String getSonarPositionMoveDownR()
        {
            return this.SonarPositionMoveDownR;
        }

        public String getSonarPositionMoveUpR()
        {
            return this.SonarPositionMoveUpR;
        }

        public String getMoveSonarPositionToSelfCheckR()
        {
            return this.MoveSonarPositionToSelfCheckR;
        }

        public String getSonarPositionSelfCheckR()
        {
            return this.SonarPositionSelfCheckR;
        }

        public String getSonarPositionMaintainR()
        {
            return this.SonarPositionMaintainR;
        }
        //

        //左
        public String getSonarPositionMoveDownL()
        {
            return this.SonarPositionMoveDownL;
        }

        public String getSonarPositionMoveUpL()
        {
            return this.SonarPositionMoveUpL;
        }

        public String getMoveSonarPositionToSelfCheckL()
        {
            return this.MoveSonarPositionToSelfCheckL;
        }

        public String getSonarPositionSelfCheckL()
        {
            return this.SonarPositionSelfCheckL;
        }

        public String getSonarPositionMaintainL()
        {
            return this.SonarPositionMaintainL;
        }
        //

        public String getBracketLMoveUp()
        {
            return this.BracketLMoveUp;
        }

        public String getBracketLMoveDown()
        {
            return this.BracketLMoveDown;
        }

        public String getBracketLMoveStop()
        {
            return this.BracketLMoveStop;
        }

        public String getBracketRMoveUp()
        {
            return this.BracketRMoveUp;
        }

        public String getBracketRMoveDown()
        {
            return this.BracketRMoveDown;
        }

        public String getBracketRMoveStop()
        {
            return this.BracketRMoveStop;
        }

        public String getTransducerLMoveUp()
        {
            return this.TransducerLMoveUp;
        }
        public String getTransducerLMoveDown()
        {
            return this.TransducerLMoveDown;
        }
        public String getTransducerLMoveStop()
        {
            return this.TransducerLMoveStop;
        }
        public String getTransducerRMoveUp()
        {
            return this.TransducerRMoveUp;
        }
        public String getTransducerRMoveDown()
        {
            return this.TransducerRMoveDown;
        }
        public String getTransducerRMoveStop()
        {
            return this.TransducerRMoveStop;
        }

        public String getStartWashingL()
        {
            return this.StartWashingL;
        }
        public String getStopWashingL()
        {
            return this.StopWashingL;
        }
        public String getStartWashingR()
        {
            return this.StartWashingR;
        }
        public String getStopWashingR()
        {
            return this.StopWashingR;
        }

        public String getStartTankHeatingL()
        {
            return this.StartTankHeatingL;
        }
        public String getStopTankHeatingL()
        {
            return this.StopTankHeatingL;
        }
        public String getStartTankHeatingR()
        {
            return this.StartTankHeatingR;

        }
        public String getStopTankHeatingR()
        {
            return this.StopTankHeatingR;
        }
    }
}
