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
        private String SonarPositionSelfCheckL = "$SonarPositionSelfCheckL\r\n";//左水位自检测
        private String MoveSonarPositionToSelfCheckL = "$MoveSonarPositionToSelfCheckL\r\n";//左测深仪移动至自检位置
        private String SonarPositionMoveUpL = "$SonarPositionMoveUpL:\r\n";//左向上移动*秒
        private String SonarPositionMoveDownL = "$SonarPositionMoveDownL:\r\n";//左向下移动*秒
        private String SonarPositionMaintainL = "$SonarPositionMaintainL:\r\n";//左测杆自动调整

        private String SonarPositionSelfCheckR = "$SonarPositionSelfCheckR\r\n";//右水位自检测
        private String MoveSonarPositionToSelfCheckR = "$MoveSonarPositionToSelfCheckR\r\n";//右测深仪移动至自检位置
        private String SonarPositionMoveUpR = "$SonarPositionMoveUpR:\r\n";//右向上移动*秒
        private String SonarPositionMoveDownR = "$SonarPositionMoveDownR:\r\n";//右向下移动*秒
        private String SonarPositionMaintainR = "$SonarPositionMaintainR:\r\n";//右测杆自动调整

        private String BracketLMoveUp = "$BracketLMoveUp\r\n";  //左侧支架向上伸展
        private String BracketLMoveDown = "$BracketLMoveDown\r\n";  //左侧支架向下伸展
        private String BracketLMoveStop = "$BracketLMoveStop\r\n";  //左侧支架关闭

        private String BracketRMoveUp = "$BracketRMoveUp\r\n";  //右侧支架向上伸展
        private String BracketRMoveDown = "$BracketRMoveDown\r\n";  //右侧支架向下伸展
        private String BracketRMoveStop = "$BracketRMoveStop\r\n";  //右侧支架关闭

        private String TransducerLMoveUp = "$TransducerLMoveUp\r\n";    //左侧换能器向上伸展
        private String TransducerLMoveDown = "$TransducerLMoveDown\r\n";    //左侧换能器向下伸展
        private String TransducerLMoveStop = "$TransducerLMoveStop\r\n";    //左侧换能器关闭

        private String TransducerRMoveUp = "$TransducerRMoveUp\r\n";    //右侧换能器向上伸展
        private String TransducerRMoveDown = "$TransducerRMoveDown\r\n";    //右侧换能器向下伸展
        private String TransducerRMoveStop = "$TransducerRMoveStop\r\n";    //右侧换能器关闭

        private String StartWashingL = "$StartWashingL\r\n";    //左侧冲洗装置启动
        private String StopWashingL = "$StopWashingL\r\n";  //左侧冲洗装置关闭
        private String StartWashingR = "$StartWashingR\r\n";    //右侧冲洗装置启动
        private String StopWashingR = "$StopWashingR\r\n";  //右侧冲洗装置关闭

        private String StartTankHeatingL = "$StartTankHeatingL\r\n";    //左侧加热启动
        private String StopTankHeatingL = "$StopTankHeatingL\r\n";  //左侧加热关闭
        private String StartTankHeatingR = "$StartTankHeatingR\r\n";    //右侧加热启动
        private String StopTankHeatingR = "$StopTankHeatingR\r\n";//右侧加热关闭

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
