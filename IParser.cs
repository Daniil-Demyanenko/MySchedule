using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace job_checker;
public interface IParser
{
    public List<JobInfo> Parse(string path);
}
