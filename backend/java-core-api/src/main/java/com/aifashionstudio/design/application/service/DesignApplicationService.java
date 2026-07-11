package com.aifashionstudio.design.application.service;

import com.aifashionstudio.design.application.command.CreateDraftDesignCommand;
import com.aifashionstudio.design.application.command.SaveDesignCommand;
import com.aifashionstudio.design.application.dto.DesignDetailResult;
import com.aifashionstudio.design.application.dto.DesignDraftResult;
import com.aifashionstudio.design.application.dto.DesignSavedResult;
import com.aifashionstudio.design.application.dto.PagedDesignResult;

public interface DesignApplicationService {

    DesignDraftResult createDraft(CreateDraftDesignCommand command);

    DesignSavedResult saveDesign(SaveDesignCommand command);

    PagedDesignResult getMyDesigns(java.util.UUID customerId, int page, int pageSize);

    DesignDetailResult getDesignDetail(java.util.UUID customerId, java.util.UUID designId);
}
