import { appListType } from './applisttype.enum';
import { androidRequiredPasswordType } from './androidrequiredpasswordtype.enum';
import { webBrowserCookieSettings } from './webbrowsercookiesettings.enum';
import { appListItem } from './applistitem.interface';
import { deviceConfiguration } from './deviceconfiguration.interface';

export interface androidGeneralDeviceConfiguration extends deviceConfiguration {
  appsBlockClipboardSharing: boolean;
  appsBlockCopyPaste: boolean;
  appsBlockYouTube: boolean;
  bluetoothBlocked: boolean;
  cameraBlocked: boolean;
  cellularBlockDataRoaming: boolean;
  cellularBlockMessaging: boolean;
  cellularBlockVoiceRoaming: boolean;
  cellularBlockWiFiTethering: boolean;
  compliantAppsList: appListItem[];
  compliantAppListType: appListType;
  diagnosticDataBlockSubmission: boolean;
  locationServicesBlocked: boolean;
  googleAccountBlockAutoSync: boolean;
  googlePlayStoreBlocked: boolean;
  kioskModeBlockSleepButton: boolean;
  kioskModeBlockVolumeButtons: boolean;
  kioskModeApps: appListItem[];
  nfcBlocked: boolean;
  passwordBlockFingerprintUnlock: boolean;
  passwordBlockTrustAgents: boolean;
  passwordExpirationDays: number;
  passwordMinimumLength: number;
  passwordMinutesOfInactivityBeforeScreenTimeout: number;
  passwordPreviousPasswordBlockCount: number;
  passwordSignInFailureCountBeforeFactoryReset: number;
  passwordRequiredType: androidRequiredPasswordType;
  passwordRequired: boolean;
  powerOffBlocked: boolean;
  factoryResetBlocked: boolean;
  screenCaptureBlocked: boolean;
  deviceSharingAllowed: boolean;
  storageBlockGoogleBackup: boolean;
  storageBlockRemovableStorage: boolean;
  storageRequireDeviceEncryption: boolean;
  storageRequireRemovableStorageEncryption: boolean;
  voiceAssistantBlocked: boolean;
  voiceDialingBlocked: boolean;
  webBrowserBlockPopups: boolean;
  webBrowserBlockAutofill: boolean;
  webBrowserBlockJavaScript: boolean;
  webBrowserBlocked: boolean;
  webBrowserCookieSettings: webBrowserCookieSettings;
  wiFiBlocked: boolean;
  appsInstallAllowList: appListItem[];
  appsLaunchBlockList: appListItem[];
  appsHideList: appListItem[];
  securityRequireVerifyApps: boolean
}