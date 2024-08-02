// A script to snap MLC to field borders
// Script created by Luke Murray 01/12/2022
// [assembly: ESAPIScript(IsWriteable = true)]

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using Microsoft.VisualBasic;
[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
    public class Script
    {
        public void Execute(ScriptContext context)
        {
            foreach(var beam in context.PlanSetup.Beams)
            {
                foreach(var controlPoint in beam.ControlPoints)
                {
                    int numFromTop;
                    int numToBottom;
                    context.Patient.BeginModifications();
                    var editables = beam.GetEditableParameters();

                    // get all borders
                    double x1, x2, y1, y2;
                    x1 = controlPoint.JawPositions.X1/10;
                    x2 = controlPoint.JawPositions.X2/10;
                    y1 = controlPoint.JawPositions.Y1/10;
                    y2 = controlPoint.JawPositions.Y2/10;
                    string temp = "Borders: x1:" + x1.ToString();
                    temp += " x2:" + x2.ToString() + " y1:" + y1.ToString() + " y2:" + y2.ToString();

                    // number of MLCs at top
                    if ((y2 > 0) && (y2 < 10)) {
                        numFromTop = 10 + Convert.ToInt32(Math.Floor(20-(y2 * 2)));
                    }
                    else if (y2 >= 10){
                        numFromTop = Convert.ToInt32(Math.Floor(20-y2));
                    }
                    else{
                        numFromTop = 30 + Convert.ToInt32(Math.Floor(y2 * -2));
                    }


                    // number of MLCs to bottom
                    if ((y1 > 0) && (y1 < 10))
                    {
                        numToBottom = 10 + Convert.ToInt32(Math.Ceiling(20-(y1 * 2)));
                    }
                    else if ((y1 >= -10) && (y1 < 0))
                    {
                        numToBottom = 30 + Convert.ToInt32(Math.Ceiling(y1 * -2));
                    }
                    else
                    {
                        numToBottom = 50 + Convert.ToInt32(Math.Ceiling((y1*-1)-10));
                    }

                    temp += "\nMLCs at top: " + numFromTop.ToString() + " MLCs to bottom: " + numToBottom.ToString() + "\n";

                    // get leaf positions
                    float[,] lp = controlPoint.LeafPositions;
                    foreach(var item in lp) { 
                        temp += (item.ToString() + " ");
                    }

                    temp += "\n";

                    int i = 60;
                    string bank = "a";
                    string temp2 = "";
                    float tempMLC;
                    float[] newMLC = new float[120];
                    int a = 0;
                    foreach(var MLC in lp)
                    {
                        temp2 = MLC.ToString();
                        tempMLC = (float)MLC;
                        if (bank == "a"){
                            if ((i > numFromTop) && (i < numToBottom))
                            {
                                if (MLC < x1) {
                                    temp2 = (x1 * -10).ToString();
                                    tempMLC = (float)(x1 * -10);
                                } 
                            }
                        }

                        if (bank == "b")
                        {
                            if ((i > numFromTop) && (i < numToBottom))
                            {
                                if (MLC > x2)
                                {
                                    temp2 = (x2 * 10).ToString();
                                    
                                    tempMLC = (float)(x2 * 10);
                                }
                            }
                        }

                        if (i == 1)
                        {
                            bank = "b";
                            i = 61;
                        }
                        i--;

                        temp = temp + temp2 + " ";
                        newMLC[a] = tempMLC;
                        a++;
                    }

                    float[,] currentMLC = new float[120, 2];

                    for(int w = 0; w < 120; w++)
                    {
                        if (w < 60)
                        {
                            currentMLC[w, 0] = 0;
                            currentMLC[w, 1] = newMLC[w];
                        }
                        else
                        {
                            currentMLC[w, 0] = 1;
                            currentMLC[w, 1] = newMLC[w];
                        }
                    }

                    File.WriteAllText("WriteLines.txt", temp);
                    editables.SetAllLeafPositions(currentMLC);
                    beam.ApplyParameters(editables);
                }
            } 
        }
    }
}
