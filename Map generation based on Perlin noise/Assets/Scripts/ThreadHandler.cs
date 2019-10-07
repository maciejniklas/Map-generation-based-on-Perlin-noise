using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ThreadHandler : MonoBehaviour
{
    private static ThreadHandler instance;

    private Queue<ThreadDetails> detailsCollection = new Queue<ThreadDetails>();

    private void Awake()
    {
        instance = FindObjectOfType<ThreadHandler>();
    }

    private void Update()
    {
        if (detailsCollection.Count > 0)
        {
            for (int index = 0; index < detailsCollection.Count; index++)
            {
                ThreadDetails threadDetails = detailsCollection.Dequeue();
                threadDetails.callback(threadDetails.variable);
            }
        }
    }

    private void DetailsThread(Func<object> buildData, Action<object> callback)
    {
        object data = buildData();

        lock (detailsCollection)
        {
            detailsCollection.Enqueue(new ThreadDetails(callback, data));
        }
    }

    public static void RequestDetails(Func<object> buildData, Action<object> callback)
    {
        ThreadStart threadStart = delegate
        {
            instance.DetailsThread(buildData, callback);
        };

        new Thread(threadStart).Start();
    }

    private struct ThreadDetails
    {
        public readonly Action<object> callback;
        public readonly object variable;

        public ThreadDetails(Action<object> callback, object variable)
        {
            this.callback = callback;
            this.variable = variable;
        }
    };
}
