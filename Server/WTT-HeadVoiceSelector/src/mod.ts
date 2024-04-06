/* eslint-disable @typescript-eslint/naming-convention */

import { DependencyContainer } from "tsyringe";
import { IPreAkiLoadMod } from "@spt-aki/models/external/IPreAkiLoadMod";
import { LogTextColor } from "@spt-aki/models/spt/logging/LogTextColor";
import { WTTInstanceManager } from "./WTTInstanceManager";
import { IPostDBLoadMod } from "@spt-aki/models/external/IPostDBLoadMod";

class HeadVoiceSelector
implements IPreAkiLoadMod, IPostDBLoadMod
{
    private Instance: WTTInstanceManager = new WTTInstanceManager();
    private version: string;
    private modName = "WTT-HeadVoiceSelector";
    debug = false;

    public preAkiLoad(container: DependencyContainer): void 
    {
        this.Instance.preAkiLoad(container, this.modName);
        this.Instance.debug = this.debug;

        this.registerWTTChangeHeadRoute();
        this.registerWTTChangeVoiceRoute();

        this.Instance.logger.log(`[${this.Instance.modName}] Routes successfully loaded!`, LogTextColor.GREEN);
    }

    public postDBLoad(container: DependencyContainer): void 
    {
        this.Instance.postDBLoad(container);
    }

    private registerWTTChangeHeadRoute(): void 
    {
        this.Instance.staticRouter.registerStaticRouter(
            "WTTChangeHead",
            [
                {
                    url: "/WTT/WTTChangeHead",
                    action: (url, info, sessionId) => 
                    {
                        if (this.Instance.debug)
                        {
                            console.log(info)
                        }
                        try 
                        {
                            const pmcSaveProfile = this.Instance.saveServer.getProfile(sessionId).characters.pmc;
                            pmcSaveProfile.Customization.Head = info.Data;
                            this.Instance.saveServer.saveProfile(sessionId);
                            this.Instance.logger.log(`[${this.Instance.modName}] Profile changes saved successfully.`, LogTextColor.GREEN);
                            return JSON.stringify({ success: true });
                        }
                        catch (err) 
                        {
                            this.Instance.logger.log(`[${this.Instance.modName}] Error saving profile. ${err}` , LogTextColor.RED);
                            return JSON.stringify({ success: false });
                        }
                    }
                }
            ],
            ""
        );
    }
    
    private registerWTTChangeVoiceRoute(): void 
    {
        this.Instance.staticRouter.registerStaticRouter(
            "WTTChangeVoice",
            [
                {
                    url: "/WTT/WTTChangeVoice",
                    action: (url, info, sessionId) => 
                    {
                        if (this.Instance.debug)
                        {
                            console.log(info)
                        }
                        try 
                        {
                            const pmcSaveProfile = this.Instance.saveServer.getProfile(sessionId).characters.pmc;
                            if (this.debug)
                            {
                                this.Instance.logger.log(`[${this.Instance.modName}] Voice Name: ${info.Data}`, LogTextColor.GREEN);
                                this.Instance.logger.log(`[${this.Instance.modName}] PMC Voice before profile changes: ${pmcSaveProfile.Info.Voice}`, LogTextColor.GREEN);
                            } 
                            pmcSaveProfile.Info.Voice = info.Data;
                            if (this.debug)
                            {
                                this.Instance.logger.log(`[${this.Instance.modName}] PMC Voice after profile changes: ${pmcSaveProfile.Info.Voice}`, LogTextColor.GREEN);
                            } 
                            this.Instance.saveServer.saveProfile(sessionId);
                            this.Instance.logger.log(`[${this.Instance.modName}] Profile changes saved successfully.`, LogTextColor.GREEN);
                            return JSON.stringify({ success: true });
                        }
                        catch (err) 
                        {
                            this.Instance.logger.log(`[${this.Instance.modName}] Error saving profile. ${err}` , LogTextColor.RED);
                            return JSON.stringify({ success: false });
                        }
                    }
                }
            ],
            ""
        );
    }
}
module.exports = { mod: new HeadVoiceSelector() };
