using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Bloxstrap.Models
{
    public sealed class ExperienceEntry : INotifyPropertyChanged
    {
        private string _name = String.Empty;
        private bool _isFavorite;
        private DateTime? _lastPlayedAtUtc;

        public long PlaceId { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                    return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                if (_isFavorite == value)
                    return;
                _isFavorite = value;
                OnPropertyChanged();
            }
        }

        public DateTime AddedAtUtc { get; set; }

        public DateTime? LastPlayedAtUtc
        {
            get => _lastPlayedAtUtc;
            set
            {
                if (_lastPlayedAtUtc == value)
                    return;
                _lastPlayedAtUtc = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LastPlayedDescription));
            }
        }

        [JsonIgnore]
        public string LastPlayedDescription => LastPlayedAtUtc is DateTime playedAt
            ? String.Format(LumiFeatureStrings.Menu_Experiences_LastPlayed, playedAt.ToLocalTime().ToString("g"))
            : String.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
