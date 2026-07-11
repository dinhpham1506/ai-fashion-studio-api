package com.aifashionstudio.design.infrastructure.persistence.entity;

import com.aifashionstudio.design.domain.model.DesignLayerType;
import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.EnumType;
import jakarta.persistence.Enumerated;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

import java.math.BigDecimal;
import java.util.UUID;

@Getter
@Setter
@Builder
@Entity
@NoArgsConstructor
@AllArgsConstructor
@Table(name = "design_layers", schema = "design")
public class DesignLayerJpaEntity {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID id;

    @Column(name = "design_id", nullable = false)
    private UUID designId;

    @Enumerated(EnumType.STRING)
    @Column(name = "layer_type", nullable = false, length = 50)
    private DesignLayerType layerType;

    @Column(columnDefinition = "TEXT")
    private String content;

    @Column(name = "position_x", nullable = false, precision = 10, scale = 2)
    private BigDecimal positionX;

    @Column(name = "position_y", nullable = false, precision = 10, scale = 2)
    private BigDecimal positionY;

    @Column(nullable = false, precision = 10, scale = 2)
    private BigDecimal width;

    @Column(nullable = false, precision = 10, scale = 2)
    private BigDecimal height;

    @Column(nullable = false, precision = 10, scale = 2)
    private BigDecimal rotation;

    @Column(length = 50)
    private String color;

    @Column(name = "z_index", nullable = false)
    private int zIndex;
}
