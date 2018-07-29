#ifndef ENTT_ENTITY_ENTITY_HPP
#define ENTT_ENTITY_ENTITY_HPP


#include "../config/config.h"
#include "entt_traits.hpp"


namespace entt {


namespace internal {


/**
 * @cond TURN_OFF_DOXYGEN
 * Internal details not to be documented.
 */


template<typename Entity>
static constexpr auto null = ~typename entt_traits<Entity>::entity_type{};


struct Null {
    explicit constexpr Null() = default;

    template<typename Entity>
    constexpr operator Entity() const ENTT_NOEXCEPT {
        return null<Entity>;
    }

    constexpr bool operator==(Null) const ENTT_NOEXCEPT {
        return true;
    }

    constexpr bool operator!=(Null) const ENTT_NOEXCEPT {
        return false;
    }

    template<typename Entity>
    constexpr bool operator==(const Entity entity) const ENTT_NOEXCEPT {
        return entity == null<Entity>;
    }

    template<typename Entity>
    constexpr bool operator!=(const Entity entity) const ENTT_NOEXCEPT {
        return entity != null<Entity>;
    }
};


template<typename Entity>
constexpr bool operator==(const Entity entity, Null null) ENTT_NOEXCEPT {
    return null == entity;
}


template<typename Entity>
constexpr bool operator!=(const Entity entity, Null null) ENTT_NOEXCEPT {
    return null != entity;
}


/**
 * Internal details not to be documented.
 * @endcond TURN_OFF_DOXYGEN
 */


}


/**
 * @brief Null entity.
 *
 * There exist implicit conversions from this variable to entity identifiers of
 * any allowed type. Similarly, there exist comparision operators between the
 * null entity and any other entity identifier.
 */
constexpr auto null = internal::Null{};


}


#endif // ENTT_ENTITY_ENTITY_HPP
