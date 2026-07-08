package com.aifashionstudio.shared.exception;

import com.aifashionstudio.shared.response.ApiError;
import com.aifashionstudio.shared.response.ApiResponse;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.MethodArgumentNotValidException;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.RestControllerAdvice;

import java.util.List;
@RestControllerAdvice
// dùng để xử lý ngoại lệ toàn cục trong ứng dụng Spring Boot.
// cho phép xác định các phương thức xử lý ngoại lệ mà sẽ được áp dụng cho tất cả các controller trong ứng dụng.
// RestControllerAdvice = @ControllerAdvice + @ResponseBody(
// trong đó @ControllerAdvice là một annotation trong Spring Framework được sử dụng để xác định một lớp xử lý ngoại lệ toàn cục cho các controller trong ứng dụng.
// Nó cho phép bạn xác định các phương thức xử lý ngoại lệ mà sẽ được áp dụng cho tất cả các controller trong ứng dụng. Khi một ngoại lệ xảy ra trong bất kỳ controller nào, Spring sẽ tìm kiếm các phương thức xử lý ngoại lệ được xác định trong lớp được chú thích bằng @ControllerAdvice và gọi phương thức phù hợp để xử lý ngoại lệ đó. )
public class GlobalExceptionHandler  {
    @ExceptionHandler(ConflictException.class)
    public ResponseEntity<ApiResponse> handleConflict(ConflictException
                                                              ex) {
        // CONFLICT là trạng thái HTTP 409
        return ResponseEntity
                .status(HttpStatus.CONFLICT)
                .body(ApiResponse.error(
                        ex.getMessage(),
                        List.of(new ApiError(null, ex.getCode(),
                                ex.getMessage()))
                ));
    }

    @ExceptionHandler(BusinessRuleException.class)
    public ResponseEntity<ApiResponse>
    handleBusinessRule(BusinessRuleException ex) {
        // UNPROCESSABLE_ENTITY là trạng thái HTTP 422,
        // thường được sử dụng khi yêu cầu hợp lệ nhưng không thể xử lý do vi phạm quy tắc nghiệp vụ
        return ResponseEntity
                .status(HttpStatus.UNPROCESSABLE_ENTITY)
                .body(ApiResponse.error(
                        ex.getMessage(),
                        List.of(new ApiError(null, ex.getCode(),
                                ex.getMessage()))
                ));
    }

    @ExceptionHandler(NotFoundException.class)
    public ResponseEntity<ApiResponse> handleNotFound(NotFoundException
                                                              ex) {
        // NOT_FOUND là trạng thái HTTP 404,
        // được sử dụng khi tài nguyên được yêu cầu không tồn tại trên máy chủ
        return ResponseEntity
                .status(HttpStatus.NOT_FOUND)
                .body(ApiResponse.error(
                        ex.getMessage(),
                        List.of(new ApiError(null, ex.getCode(),
                                ex.getMessage()))
                ));
    }

    @ExceptionHandler(MethodArgumentNotValidException.class)
    public ResponseEntity<ApiResponse> handleValidation(MethodArgumentNotValidException ex) {
        List<ApiError> errors = ex.getBindingResult()
                .getFieldErrors()
                .stream()
                .map(error -> new ApiError(
                        error.getField(),
                        "VALIDATION_FAILED",
                        error.getDefaultMessage()
                ))
                .toList();

        return ResponseEntity
                .status(HttpStatus.UNPROCESSABLE_ENTITY)
                .body(ApiResponse.error("Validation failed", errors));
    }
}

