            if (_config.HideOffscreenPingText.Value)
            {
                if (!localUser.cameraRigController.sceneCam.IsObjectVisible(self.transform))
                {
                    self.pingText.alpha = 0;
                }
                else
                {
                    self.pingText.alpha = 1;
                }   
            }
