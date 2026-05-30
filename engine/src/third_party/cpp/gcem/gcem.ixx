/**
 * @file gcem.ixx
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
module;

#include <gcem.hpp>

export module gcem;

export namespace gcem
{
    constexpr inline std::int32_t VERSION_MAJOR = GCEM_VERSION_MAJOR;
    constexpr inline std::int32_t VERSION_MINOR = GCEM_VERSION_MINOR;
    constexpr inline std::int32_t VERSION_PATCH = GCEM_VERSION_PATCH;

    using gcem::common_return_t;
    using gcem::common_t;
    using gcem::GCLIM;
    using gcem::llint_t;
    using gcem::return_t;
    using gcem::uint_t;
    using gcem::ullint_t;

    constexpr inline auto LOG_2 = GCEM_LOG_2;
    constexpr inline auto LOG_10 = GCEM_LOG_10;
    constexpr inline auto PI = GCEM_PI;
    constexpr inline auto LOG_PI = GCEM_LOG_PI;
    constexpr inline auto LOG_2PI = GCEM_LOG_2PI;
    constexpr inline auto LOG_SQRT_2PI = GCEM_LOG_SQRT_2PI;
    constexpr inline auto SQRT_2 = GCEM_SQRT_2;
    constexpr inline auto HALF_PI = GCEM_HALF_PI;
    constexpr inline auto SQRT_PI = GCEM_SQRT_PI;
    constexpr inline auto SQRT_HALF_PI = GCEM_SQRT_HALF_PI;
    constexpr inline auto E = GCEM_E;
    constexpr inline auto ERF_MAX_ITER = GCEM_ERF_MAX_ITER;
    constexpr inline auto ERF_INV_MAX_ITER = GCEM_ERF_INV_MAX_ITER;
    constexpr inline auto EXP_MAX_ITER_SMALL = GCEM_EXP_MAX_ITER_SMALL;
    constexpr inline auto LOG_MAX_ITER_SMALL = GCEM_LOG_MAX_ITER_SMALL;
    constexpr inline auto LOG_MAX_ITER_BIG = GCEM_LOG_MAX_ITER_BIG;
    constexpr inline auto INCML_BETA_TOL = GCEM_INCML_BETA_TOL;
    constexpr inline auto INCML_BETA_MAX_ITER = GCEM_INCML_BETA_MAX_ITER;
    constexpr inline auto INCML_BETA_INV_MAX_ITER = GCEM_INCML_BETA_INV_MAX_ITER;
    constexpr inline auto INCML_GAMMA_MAX_ITER = GCEM_INCML_GAMMA_MAX_ITER;
    constexpr inline auto INCML_GAMMA_INV_MAX_ITER = GCEM_INCML_GAMMA_INV_MAX_ITER;
    constexpr inline auto SQRT_MAX_ITER = GCEM_SQRT_MAX_ITER;
    constexpr inline auto INV_SQRT_MAX_ITER = GCEM_INV_SQRT_MAX_ITER;
    constexpr inline auto TAN_MAX_ITER = GCEM_TAN_MAX_ITER;
    constexpr inline auto TANH_MAX_ITER = GCEM_TANH_MAX_ITER;

    constexpr auto signbit(auto x)
    {
        return GCEM_SIGNBIT(x);
    }

    constexpr auto copysign(auto x, auto y)
    {
        return GCEM_COPYSIGN(x, y);
    }

    using gcem::abs;
    using gcem::acos;
    using gcem::acosh;
    using gcem::asin;
    using gcem::asinh;
    using gcem::atan;
    using gcem::atan2;
    using gcem::atanh;
    using gcem::beta;
    using gcem::binomial_coef;
    using gcem::ceil;
    using gcem::copysign;
    using gcem::cos;
    using gcem::cosh;
    using gcem::erf;
    using gcem::erf_inv;
    using gcem::exp;
    using gcem::expm1;
    using gcem::fabs;
    using gcem::fabsf;
    using gcem::fabsl;
    using gcem::factorial;
    using gcem::floor;
    using gcem::fmod;
    using gcem::gcd;
    using gcem::hypot;
    using gcem::incomplete_beta;
    using gcem::incomplete_beta_inv;
    using gcem::incomplete_gamma;
    using gcem::incomplete_gamma_inv;
    using gcem::inv_sqrt;
    using gcem::lbeta;
    using gcem::lcm;
    using gcem::lgamma;
    using gcem::lmgamma;
    using gcem::log;
    using gcem::log10;
    using gcem::log1p;
    using gcem::log2;
    using gcem::log_binomial_coef;
    using gcem::max;
    using gcem::min;
    using gcem::pow;
    using gcem::round;
    using gcem::sgn;
    using gcem::signbit;
    using gcem::sin;
    using gcem::sinh;
    using gcem::sqrt;
    using gcem::tan;
    using gcem::tanh;
    using gcem::tgamma;
    using gcem::trunc;
} // namespace gcem
