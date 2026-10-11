// Public Firebase Web App configuration only. Never put an Admin/service-account key here.
window.DragonTowerFirebaseConfig = {
  enabled: false, // Offline release; retained only for local save identification.
  recoveryEnabled: false, // Enable after deploying the recovery function and SMTP secrets.
  leaderboardEnabled: false, // Enable after Functions + App Check deployment. No direct client score writes.
  appCheckSiteKey: '',
  functionsRegion: 'asia-northeast3',
  environment: 'development',
  firebase: { apiKey: 'AIzaSyC0a_MaUusoL0ul8IMvr3IVH5NcEFoS6W8', authDomain: 'dragon-tower-da915.firebaseapp.com', projectId: 'dragon-tower-da915', appId: '1:756886782750:web:f6e142a63ae17d39557f0f' }
};
