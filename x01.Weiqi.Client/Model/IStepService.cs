// --------------------------------------------
// Copyright (c) 2011 by x01(china_x01@qq.com)
// --------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x01.Weiqi.Model
{
    public interface IStepService
    {
        int[] GetIds();
        string GetContent(int id);
		void SaveStep(Step step);
    }
}
