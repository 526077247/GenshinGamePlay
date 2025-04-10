using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

namespace Nebukam.JobAssist.Examples
{

    public class JobStackingExample : MonoBehaviour
    {

        JobStack jobStack = new JobStack();

        void Start()
        {
            
            SimpleJobHandler A = new SimpleJobHandler("A");
            SimpleJobHandler B = new SimpleJobHandler("B");
            SimpleJobHandler C = new SimpleJobHandler("C");
            SimpleJobHandler D = new SimpleJobHandler("D");

            jobStack.Add(A);
            jobStack.Add(B);
            jobStack.Add(C);
            jobStack.Add(D);
            
        }

        void Update()
        {
            //Schedule like a normal job.
            jobStack.Schedule(Time.deltaTime);
        }

        void LateUpdate()
        {
            //Completeting the stack will ensure Complete() is called
            //in order in each registered job.
            jobStack.Complete();
        }

    }

    public class SimpleJobHandler : JobHandler<SimpleJob>
    {

        protected int index = 1;
        protected string m_id = string.Empty;

        protected NativeArray<int> m_result = new NativeArray<int>(1, Allocator.Persistent);

        public SimpleJobHandler(string ID)
        {
            m_id = ID;
        }

        protected override void Prepare(ref SimpleJob job, float delta)
        {
            //Prepare the job, fill values etc
            job.val = index;
            job.results = m_result;
        }

        protected override void Apply(ref SimpleJob job)
        {
            //Apply the results of the job
            index = m_result[0];
            Debug.Log(m_id+" : "+ index);
        }
        
    }

    public struct SimpleJob : IJob
    {

        public int val;
        public NativeArray<int> results;

        public void Execute()
        {
            results [0] = val + 1;
            //Execute your job...
        }
    }


}
