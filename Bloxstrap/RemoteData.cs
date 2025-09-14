using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace Bloxstrap
{
    public class RemoteDataManager : JsonManager<RemoteDataBase>
    {
        public override string ClassName => nameof(RemoteDataManager);

        public override string LOG_IDENT_CLASS => ClassName;

        public override string FileLocation => Path.Combine(Paths.Base, "Data.json");

        public bool Changed => !OriginalProp.Equals(Prop);

        public GenericTriState LoadedState = GenericTriState.Unknown;

        public event EventHandler DataLoaded = null!;

        public void Subscribe(EventHandler Handler)
        {
            switch (LoadedState)
            {
                case GenericTriState.Unknown:
                    DataLoaded += Handler;
                    break;
                case GenericTriState.Successful:
                    Handler(this, EventArgs.Empty);
                    break;
                default:
                    Handler(this, EventArgs.Empty); // data loading most likely failed but we still have the default/local config
                    break;
            }
        }

        public async Task WaitUntilDataFetched()
        {
            while (LoadedState == GenericTriState.Unknown)        
                await Task.Delay(100);    

            return;
        }

        // remember that our data isnt necessary, we can run it asynchronously 
        public async Task LoadData()
        {
            const string LOG_IDENT = "RemoteData::LoadData";
            if (App.Settings.Prop.ForceLocalData)
            {
                App.Logger.WriteLine(LOG_IDENT, "Force loading local data");
                this.Load(false);
            } else
                try
                {
                    Prop = await Http.GetJson<RemoteDataBase>(App.ProjectRemoteDataLink);
                    App.Logger.WriteLine(LOG_IDENT, "Remote data loaded");
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Could not load remote data");
                    App.Logger.WriteException(LOG_IDENT, ex);

                    App.Logger.WriteLine(LOG_IDENT, "Loading local data");
                    this.Load(false);

                    LoadedState = GenericTriState.Failed;
                }

            LoadedState = GenericTriState.Successful;

            DataLoaded?.Invoke(this, EventArgs.Empty);
            this.Save();
        }
    }
}
