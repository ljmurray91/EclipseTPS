// A script to quickly view predicted rectal dose using the equation given by Worcester NHS
// Requires structures 'Rectum' and '[T]Rec_PTV1_Ovl'
// Script created by Luke Murray 09/11/2022

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace VMS.TPS
{
	public class Script
	{
		public void Execute(ScriptContext context)
		{
			StructureSet structureSet = context.StructureSet;
			try{
				// Getting rectum structures and volumes
				Structure rectum = structureSet.Structures.Where(s => s.Id.Contains("Rectum")).First();
				double rectVol = rectum.Volume;
				Structure rectumOvl = structureSet.Structures.Where(s => s.Id.Contains("[T]Rec_PTV1_Ovl")).First();
				double rectOvlVol = rectumOvl.Volume;

				// Math for predictor
				double rectRatio = rectOvlVol / rectVol;
				double prediction = 60* (0.3+(1.05*(1 - Math.Exp(-1.25*rectRatio))));

				// Printing results
				MessageBox.Show("Rectum Volume = " + rectVol.ToString("0.##") +"cc" + "\nRectum Overlap = " + rectOvlVol.ToString("0.##")+"cc" + "\nPredicted Rectal Mean = "+ Math.Round(prediction, 2) + "Gy", "Rectum Mean Predictor");
			}
			catch (Exception){
				MessageBox.Show("Script failed.\n\nCheck structures exist and naming convention.\n'Rectum' and '[T]Rec_PTV1_Ovl' are needed.");
			}
		}
	}
}
