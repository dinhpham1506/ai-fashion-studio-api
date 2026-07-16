package com.aifashionstudio.shared.exception;

public class ServiceUnavailableException extends AppException {
  public ServiceUnavailableException(String code, String message) {
    super(code, message);
  }
}
