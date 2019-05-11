namespace Assets.StarSystem.Generation.Planet.Threads
{
    public class LODCalculatorThread : BaseThread
    {
        //the calculator thread is the main processing power behind mesh generation

        private bool cancel;

        public LODCalculatorThread(SystemController controller) : base(controller)
        {
            cancel = false;
        }

        protected override void Action()
        {
            //see if region needs calculation
            if (controller.regionCalculationStack.Count > 0)
            {
                Region region;
                lock (controller.regionCalculationStack)
                {
                    region = controller.regionCalculationStack.Remove();
                }
                //check if thread is still running and not cancelled
                if (region != null && isRunning && !cancel)
                {
                    //if another thread is processing this region, cancel
                    if (region.processing) Cancel();

                    if (!cancel) //check if still running
                    {
                        region.processing = true;
                        //generate the mesh
                        region.CreateMesh();
                    }

                    if (!cancel) //check if still running
                    {
                        //add calculated region to render queue, so the main thread can place the mesh in the world
                        lock (controller.regionRenderQueue)
                        {
                            controller.regionRenderQueue.Add(region.Key, region, region.LOD);
                        }
                    }
                    region.processing = false;
                }
                cancel = false;
                System.Threading.Thread.Sleep(20);
            }
            else
            {
                System.Threading.Thread.Sleep(50);
            }
        }

        public void Cancel()
        {
            cancel = true;
        }
    }
}
